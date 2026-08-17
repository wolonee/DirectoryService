using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.MultipartUpload.CompleteMultipartUpload;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.MultipartUpload;

public sealed record CompleteMultipartUploadCommand(CompleteMultipartUploadRequest Request) : ICommand;

public sealed class CompleteMultipartUploadValidator : AbstractValidator<CompleteMultipartUploadCommand>
{
    public CompleteMultipartUploadValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CompleteMultipartUploadCommand.Request)));

        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.FileId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(CompleteMultipartUploadRequest.FileId)));

            RuleFor(command => command.Request.UploadId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(CompleteMultipartUploadRequest.UploadId)));

            RuleFor(command => command.Request.Parts)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(CompleteMultipartUploadRequest.Parts)));

            RuleForEach(command => command.Request.Parts)
                .ChildRules(part =>
                {
                    part.RuleFor(value => value.PartNumber)
                        .GreaterThan(0)
                        .WithError(GeneralErrors.ValueIsInvalid(nameof(PartETagDto.PartNumber)));

                    part.RuleFor(value => value.ETag)
                        .NotEmpty()
                        .WithError(GeneralErrors.ValueIsRequired(nameof(PartETagDto.ETag)));
                });
        });
    }
}

public sealed class CompleteMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/complete", async Task<EndpointResult<CompleteMultipartUploadResponse>>(
            [FromBody] CompleteMultipartUploadRequest request,
            [FromServices] ICommandHandler<CompleteMultipartUploadResponse, CompleteMultipartUploadCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteMultipartUploadCommand(request);

            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class CompleteMultipartUploadHandler
    : ICommandHandler<CompleteMultipartUploadResponse, CompleteMultipartUploadCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IVideoProcessingScheduler _scheduler;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CompleteMultipartUploadCommand> _validator;
    private readonly ILogger<CompleteMultipartUploadHandler> _logger;

    public CompleteMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetRepository mediaAssetRepository,
        ICurrentUser currentUser,
        IValidator<CompleteMultipartUploadCommand> validator,
        ILogger<CompleteMultipartUploadHandler> logger,
        ITransactionManager transactionManager,
        IVideoProcessingScheduler scheduler)
    {
        _s3Provider = s3Provider;
        _mediaAssetRepository = mediaAssetRepository;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
        _scheduler = scheduler;
    }

    public async Task<Result<CompleteMultipartUploadResponse, Errors>> Handle(
        CompleteMultipartUploadCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        var request = command.Request;

        var mediaAssetResult = await _mediaAssetRepository.GetByIdAsync(request.FileId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        var asset = mediaAssetResult.Value;

        if (asset.Owner.UploaderId != _currentUser.UserId)
            return MediaAssetErrors.WrongUploader(request.FileId).ToErrors();

        if (asset.Status is MediaStatus.UPLOADED or MediaStatus.READY)
            return MediaAssetErrors.AlreadyCompleted(request.FileId).ToErrors();

        if (asset.Status != MediaStatus.UPLOADING)
            return MediaAssetErrors.InvalidStatus(request.FileId, asset.Status).ToErrors();

        if (asset.MultipartUploadId != request.UploadId)
            return MediaAssetErrors.InvalidMultipartUploadId(request.FileId).ToErrors();

        if (asset.MediaData.ExpectedChunksCount != request.Parts.Count)
            return GeneralErrors.Failure("Expected chunks count mismatch").ToErrors();

        if (request.Parts.Any(part =>
                part.PartNumber > asset.MediaData.ExpectedChunksCount)
            || request.Parts.Select(part => part.PartNumber).Distinct().Count() != request.Parts.Count)
        {
            return GeneralErrors.Failure("Multipart parts are invalid").ToErrors();
        }

        var completeMultipartResult = await _s3Provider.CompleteMultipartUploadAsync(
            asset.UploadKey,
            asset.MultipartUploadId,
            request.Parts,
            cancellationToken);
        if (completeMultipartResult.IsFailure)
        {
            _logger.LogError("Multipart upload failed for media asset {MediaAssetId}", request.FileId);
            return completeMultipartResult.Error.ToErrors();
        }

        var metadataResult = await _s3Provider.GetObjectMetadataAsync(asset.UploadKey, cancellationToken);
        if (metadataResult.IsFailure)
        {
            _logger.LogError("Object metadata was not found for media asset {MediaAssetId}", request.FileId);
            return metadataResult.Error.ToErrors();
        }

        var metadata = metadataResult.Value;

        if (metadata.ContentLength != asset.MediaData.Size)
        {
            _logger.LogError("File size does not match for media asset {MediaAssetId}", request.FileId);
            return await MarkAssetFailedAsync(
                asset,
                MediaAssetErrors.SizeMismatch(asset.MediaData.Size, metadata.ContentLength),
                cancellationToken);
        }

        if (metadata.ContentType != asset.MediaData.ContentType.Value)
        {
            _logger.LogError("File type does not match for media asset {MediaAssetId}", request.FileId);
            return await MarkAssetFailedAsync(
                asset,
                MediaAssetErrors.ContentTypeMismatch(asset.MediaData.ContentType.Value, metadata.ContentType ?? string.Empty),
                cancellationToken);
        }

        Result<StorageReference, Error> storageReferenceResult = StorageReference.Create(
            asset.UploadKey,
            metadata.ContentLength,
            metadata.ContentType ?? string.Empty,
            metadata.ETag,
            metadata.Checksum,
            metadata.LastModified);
        if (storageReferenceResult.IsFailure)
            return storageReferenceResult.Error.ToErrors();

        var markUploadResult = asset.MarkUploaded(DateTime.UtcNow);
        if (markUploadResult.IsFailure)
            return markUploadResult.Error.ToErrors();

        bool requiresProcessing = asset.RequiresProcessing();
        if (!requiresProcessing)
        {
            var markReadyResult = asset.MarkReady(asset.UploadKey, storageReferenceResult.Value, DateTime.UtcNow);
            if (markReadyResult.IsFailure)
                return markReadyResult.Error.ToErrors();

            _logger.LogInformation("MediaAssetId: {MediaAssetId} doesn't need processing", asset.Id);
        }

        // Сначала фиксируем изменения БД (SaveChangesAsync атомарен сам по себе),
        // и только потом планируем job — иначе Quartz-триггер может остаться при откате БД.
        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();

        if (requiresProcessing)
        {
            var scheduleResult = await _scheduler.ScheduleAsync(asset, cancellationToken);
            if (scheduleResult.IsFailure)
                return scheduleResult.Error.ToErrors();
        }

        _logger.LogInformation("File {FileId} saved", request.FileId);

        return new CompleteMultipartUploadResponse { FileId = request.FileId };
    }

    private async Task<Result<CompleteMultipartUploadResponse, Errors>> MarkAssetFailedAsync(
        MediaAsset asset,
        Error resultError,
        CancellationToken cancellationToken)
    {
        var markFailedResult = asset.MarkFailed(DateTime.UtcNow);
        if (markFailedResult.IsFailure)
            return markFailedResult.Error.ToErrors();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        return resultError.ToErrors();
    }
}
