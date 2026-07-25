using CSharpFunctionalExtensions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.SimpleUpload;

/// <summary>
/// Команда подтверждения уже загруженного объекта.
/// </summary>
/// <param name="FileId">Идентификатор asset-а, созданного через initiate.</param>
/// <remarks>
/// UploaderId намеренно не передаётся из запроса: обработчик берёт его из
/// <see cref="FileService.Core.Abstractions.ICurrentUser"/>.
/// </remarks>
public sealed record CompleteUploadCommand(Guid FileId);
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
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CompleteUploadHandler> _logger;

    public CompleteUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        IS3Provider s3Provider,
        ICurrentUser currentUser,
        ILogger<CompleteUploadHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<CompleteUploadResponse, Error>> Handle(CompleteUploadCommand command, CancellationToken cancellationToken)
    {
        var fileId = command.FileId;
        
        if (fileId == Guid.Empty)
        {
            _logger.LogError("FileId is empty");
            return GeneralErrors.ValueIsInvalid("File Id");
        }

        var assetResult = await _mediaAssetRepository.GetByIdAsync(fileId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;
        
        var asset = assetResult.Value;

        if (asset.Owner.UploaderId != _currentUser.UserId)
            return MediaAssetErrors.WrongUploader(fileId);

        if (asset.Status == MediaStatus.READY)
            return MediaAssetErrors.AlreadyCompleted(fileId);

        if (asset.Status != MediaStatus.UPLOADING)
            return MediaAssetErrors.InvalidStatus(fileId, asset.Status);

        var metadataResult = await _s3Provider.GetObjectMetadataAsync(asset.RawKey, cancellationToken);
        if (metadataResult.IsFailure)
            return metadataResult.Error;
        
        var metadata = metadataResult.Value;

        if (metadata.ContentLength != asset.MediaData.Size)
        {
            _logger.LogError("File size does not match");
            return MediaAssetErrors.SizeMismatch(asset.MediaData.Size, metadata.ContentLength);
        }

        if (metadata.ContentType != asset.MediaData.ContentType.Value)
        {
            _logger.LogError("File type does not match");
            return MediaAssetErrors.ContentTypeMismatch(asset.MediaData.ContentType.Value, metadata.ContentType ?? string.Empty);
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
            return storageReferenceResult.Error;
        }

        if (asset.AssetType == AssetType.PREVIEW)
        {
            PreviewAsset preview = (PreviewAsset)asset;
            UnitResult<Error> completeResult = preview.CompleteUpload(storageReferenceResult.Value, DateTime.UtcNow);
            if (completeResult.IsFailure)
                return completeResult.Error;
        }

        var saveChangesResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;
        
        var response = new CompleteUploadResponse(asset.Id, asset.Status.ToString(), metadataResult.Value);

        return response;
    }
}













