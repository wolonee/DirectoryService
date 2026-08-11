using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.MultipartUpload.AbortMultipartUpload;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.S3Entities;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.MultipartUpload;

public sealed record AbortMultipartUploadCommand(AbortMultipartUploadRequest Request) : ICommand;

public sealed class AbortMultipartUploadValidator : AbstractValidator<AbortMultipartUploadCommand>
{
    public AbortMultipartUploadValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(AbortMultipartUploadCommand.Request)));

        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.FileId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(AbortMultipartUploadRequest.FileId)));

            RuleFor(command => command.Request.UploadId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(AbortMultipartUploadRequest.UploadId)));
        });
    }
}

public sealed class AbortMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/abort", async Task<EndpointResult<AbortMultipartUploadResponse>>(
            [FromBody] AbortMultipartUploadRequest request,
            [FromServices] ICommandHandler<AbortMultipartUploadResponse, AbortMultipartUploadCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AbortMultipartUploadCommand(request);

            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class AbortMultipartUploadHandler
    : ICommandHandler<AbortMultipartUploadResponse, AbortMultipartUploadCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<AbortMultipartUploadCommand> _validator;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;

    public AbortMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetRepository mediaAssetRepository,
        ICurrentUser currentUser,
        IValidator<AbortMultipartUploadCommand> validator,
        ILogger<AbortMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaAssetRepository = mediaAssetRepository;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AbortMultipartUploadResponse, Errors>> Handle(
        AbortMultipartUploadCommand command,
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

        if (asset.Status == MediaStatus.DELETED)
            return MediaAssetErrors.AlreadyDeleted(asset.Id).ToErrors();
        
        if (asset.Status != MediaStatus.UPLOADING)
            return MediaAssetErrors.InvalidStatus(asset.Id, asset.Status).ToErrors();

        if (asset.MultipartUploadId != request.UploadId)
            return MediaAssetErrors.InvalidMultipartUploadId(asset.Id).ToErrors();
        
        var deleteResult = await _s3Provider.AbortMultipartUploadAsync(asset.RawKey, asset.MultipartUploadId, cancellationToken);
        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();
        
        var markDeletedResult = asset.MarkDeleted(DateTime.UtcNow);
        if (markDeletedResult.IsFailure)
            return markDeletedResult.Error.ToErrors();
        
        var saveChangesResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();
        
        _logger.LogInformation(
            "Multipart upload aborted for media asset {MediaAssetId}", asset.Id);
        
        var response = new AbortMultipartUploadResponse
        {
            FileId = asset.Id, 
            Status = asset.Status.ToString().ToLowerInvariant(),
        };

        return response;
    }

}
