using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.SimpleUpload;

public sealed record CompleteUploadCommand(Guid FileId) : ICommand;

/// <summary>
/// Команда подтверждения уже загруженного объекта.
/// </summary>
/// <param name="FileId">Идентификатор asset-а, созданного через initiate.</param>
/// <remarks>
/// UploaderId намеренно не передаётся из запроса: обработчик берёт его из
/// <see cref="FileService.Core.Abstractions.ICurrentUser"/>.
/// </remarks>
public sealed class CompleteUploadValidator : AbstractValidator<CompleteUploadCommand>
{
    public CompleteUploadValidator()
    {
        RuleFor(command => command.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CompleteUploadCommand.FileId)));
    }
}

public sealed class CompleteUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/{fileId:guid}/complete", async Task<EndpointResult<CompleteUploadResponse>> (
            [FromRoute] Guid fileId,
            [FromServices] CompleteUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteUploadCommand(fileId);
            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class CompleteUploadHandler
    : ICommandHandler<CompleteUploadResponse, CompleteUploadCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CompleteUploadCommand> _validator;
    private readonly ILogger<CompleteUploadHandler> _logger;

    public CompleteUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        IS3Provider s3Provider,
        ICurrentUser currentUser,
        IValidator<CompleteUploadCommand> validator,
        ILogger<CompleteUploadHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CompleteUploadResponse, Errors>> Handle(CompleteUploadCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        var fileId = command.FileId;

        var assetResult = await _mediaAssetRepository.GetByIdAsync(fileId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();
        
        var asset = assetResult.Value;

        if (asset.Owner.UploaderId != _currentUser.UserId)
            return MediaAssetErrors.WrongUploader(fileId).ToErrors();

        if (asset.Status == MediaStatus.READY)
            return MediaAssetErrors.AlreadyCompleted(fileId).ToErrors();

        if (asset.Status != MediaStatus.UPLOADING)
            return MediaAssetErrors.InvalidStatus(fileId, asset.Status).ToErrors();

        // FS-3 завершает только простой preview upload. Видео completion относится к следующему flow.
        if (asset.AssetType != AssetType.PREVIEW)
            return GeneralErrors.ValueIsInvalid(nameof(asset.AssetType)).ToErrors();

        var metadataResult = await _s3Provider.GetObjectMetadataAsync(asset.RawKey, cancellationToken);
        if (metadataResult.IsFailure)
            return metadataResult.Error.ToErrors();
        
        var metadata = metadataResult.Value;

        if (metadata.ContentLength != asset.MediaData.Size)
        {
            _logger.LogError("File size does not match");
            return MediaAssetErrors.SizeMismatch(asset.MediaData.Size, metadata.ContentLength).ToErrors();
        }

        if (metadata.ContentType != asset.MediaData.ContentType.Value)
        {
            _logger.LogError("File type does not match");
            return MediaAssetErrors.ContentTypeMismatch(asset.MediaData.ContentType.Value, metadata.ContentType ?? string.Empty).ToErrors();
        }
        
        Result<StorageReference, Error> storageReferenceResult = StorageReference.Create(
            asset.RawKey,
            metadata.ContentLength,
            metadata.ContentType ?? string.Empty,
            metadata.ETag,
            metadata.Checksum,
            metadata.LastModified);
        if (storageReferenceResult.IsFailure)
        {
            return storageReferenceResult.Error.ToErrors();
        }

        PreviewAsset preview = (PreviewAsset)asset;
        UnitResult<Error> completeResult = preview.CompleteUpload(storageReferenceResult.Value, DateTime.UtcNow);
        if (completeResult.IsFailure)
            return completeResult.Error.ToErrors();

        var saveChangesResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();
        
        var response = new CompleteUploadResponse(asset.Id, asset.Status.ToString(), metadataResult.Value);

        return response;
    }

}





