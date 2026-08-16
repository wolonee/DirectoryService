using System.Data;
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
using FileService.VideoProcessing.Jobs;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Quartz;

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
    private readonly IEnumerable<IProcessingJobFactory> _jobFactories;
    private readonly ISchedulerFactory _schedulerFactory;
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
        IEnumerable<IProcessingJobFactory> jobFactories,
        ISchedulerFactory schedulerFactory)
    {
        _s3Provider = s3Provider;
        _mediaAssetRepository = mediaAssetRepository;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
        _jobFactories = jobFactories;
        _schedulerFactory = schedulerFactory;
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

        try
        {
            IDbTransaction transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);

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
                var markFailedResult = asset.MarkFailed(DateTime.UtcNow);
                if (markFailedResult.IsFailure)
                    return markFailedResult.Error.ToErrors();

                var failedSaveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (failedSaveChangesResult.IsFailure)
                    return failedSaveChangesResult.Error.ToErrors();

                return MediaAssetErrors.SizeMismatch(asset.MediaData.Size, metadata.ContentLength).ToErrors();
            }

            if (metadata.ContentType != asset.MediaData.ContentType.Value)
            {
                _logger.LogError("File type does not match for media asset {MediaAssetId}", request.FileId);
                var markFailedResult = asset.MarkFailed(DateTime.UtcNow);
                if (markFailedResult.IsFailure)
                    return markFailedResult.Error.ToErrors();

                var failedSaveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (failedSaveChangesResult.IsFailure)
                    return failedSaveChangesResult.Error.ToErrors();

                return MediaAssetErrors
                    .ContentTypeMismatch(asset.MediaData.ContentType.Value, metadata.ContentType ?? string.Empty)
                    .ToErrors();
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

            if (asset.RequiresProcessing())
            {
                var factory = _jobFactories.FirstOrDefault(f => f.CanProcess(asset));
                if (factory is null)
                {
                    _logger.LogError("No processing job factory found for MediaAssetId: {MediaAssetId}", asset.Id);
                    return GeneralErrors.Failure("No processing job factory found").ToErrors();
                }

                IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

                IJobDetail job = factory.CreateJob(asset);
                ITrigger trigger = factory.CreateTrigger(asset);

                await scheduler.ScheduleJob(job, trigger, cancellationToken);

                _logger.LogInformation("Scheduled processing job for MediaAssetId: {MediaAssetId}", asset.Id);
            }
            else
            {
                var markReadyResult = asset.MarkReady(asset.UploadKey, storageReferenceResult.Value, DateTime.UtcNow);
                if (markReadyResult.IsFailure)
                    return markReadyResult.Error.ToErrors();

                _logger.LogInformation("MediaAssetId: {MediaAssetId} doesn't need processing", asset.Id);
            }

            var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesResult.IsFailure)
                return saveChangesResult.Error.ToErrors();

            _logger.LogInformation("File {FileId} saved", request.FileId);
            
            transaction.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing multipart upload");
            return GeneralErrors.Failure("Error completing multipart upload").ToErrors();
        }
        
        var response = new CompleteMultipartUploadResponse { FileId = request.FileId };

        return response;
    }
}
