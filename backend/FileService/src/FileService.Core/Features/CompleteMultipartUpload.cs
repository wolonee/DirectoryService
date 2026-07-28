using CSharpFunctionalExtensions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features;

public record CompleteMultipartUploadCommand(CompleteMultipartUploadRequest Request);

public sealed class CompleteMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/complete", async Task<EndpointResult<CompleteMultipartUploadResponse>>(
            [FromBody] CompleteMultipartUploadRequest request,
            [FromServices] CompleteMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteMultipartUploadCommand(request);

            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class CompleteMultipartUploadHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CompleteMultipartUploadHandler> _logger;

    public CompleteMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetRepository mediaAssetRepository,
        ICurrentUser currentUser,
        ILogger<CompleteMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaAssetRepository = mediaAssetRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<CompleteMultipartUploadResponse, Error>> Handle(
        CompleteMultipartUploadCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var mediaAssetResult = await _mediaAssetRepository.GetByIdAsync(request.FileId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var asset = mediaAssetResult.Value;

        if (asset.Owner.UploaderId != _currentUser.UserId)
            return MediaAssetErrors.WrongUploader(request.FileId);

        if (asset.Status is MediaStatus.UPLOADED or MediaStatus.READY)
            return MediaAssetErrors.AlreadyCompleted(request.FileId);

        if (asset.Status != MediaStatus.UPLOADING)
            return MediaAssetErrors.InvalidStatus(request.FileId, asset.Status);

        if (asset.MultipartUploadId != request.UploadId)
            return MediaAssetErrors.InvalidMultipartUploadId(request.FileId);

        if (asset.MediaData.ExpectedChunksCount != request.Parts.Count)
            return GeneralErrors.Failure("Expected chunks count mismatch");

        if (request.Parts.Any(part =>
                part.PartNumber < 1
                || part.PartNumber > asset.MediaData.ExpectedChunksCount
                || string.IsNullOrWhiteSpace(part.ETag))
            || request.Parts.Select(part => part.PartNumber).Distinct().Count() != request.Parts.Count)
        {
            return GeneralErrors.Failure("Multipart parts are invalid");
        }

        var completeMultipartResult = await _s3Provider.CompleteMultipartUploadAsync(
            asset.RawKey,
            asset.MultipartUploadId,
            request.Parts,
            cancellationToken);
        if (completeMultipartResult.IsFailure)
        {
            _logger.LogError("Multipart upload failed for media asset {MediaAssetId}", request.FileId);
            return completeMultipartResult.Error;
        }
        
        var metadataResult = await _s3Provider.GetObjectMetadataAsync(asset.RawKey, cancellationToken);
        if (metadataResult.IsFailure)
        {
            _logger.LogError("Object metadata was not found for media asset {MediaAssetId}", request.FileId);
            return metadataResult.Error;
        }

        var metadata = metadataResult.Value;

        if (metadata.ContentLength != asset.MediaData.Size)
        {
            _logger.LogError("File size does not match for media asset {MediaAssetId}", request.FileId);
            var markFailedResult = asset.MarkFailed(DateTime.UtcNow);
            if (markFailedResult.IsFailure)
                return markFailedResult.Error;

            var failedSaveChangesResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
            if (failedSaveChangesResult.IsFailure)
                return failedSaveChangesResult.Error;

            return MediaAssetErrors.SizeMismatch(asset.MediaData.Size, metadata.ContentLength);
        }

        if (metadata.ContentType != asset.MediaData.ContentType.Value)
        {
            _logger.LogError("File type does not match for media asset {MediaAssetId}", request.FileId);
            var markFailedResult = asset.MarkFailed(DateTime.UtcNow);
            if (markFailedResult.IsFailure)
                return markFailedResult.Error;

            var failedSaveChangesResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
            if (failedSaveChangesResult.IsFailure)
                return failedSaveChangesResult.Error;

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
            return storageReferenceResult.Error;

        var markUploadResult = asset.MarkUploaded(DateTime.UtcNow);
        if (markUploadResult.IsFailure)
            return markUploadResult.Error;

        var markReadyResult = asset.MarkReady(asset.RawKey, storageReferenceResult.Value, DateTime.UtcNow);
        if (markReadyResult.IsFailure)
            return markReadyResult.Error;

        var saveChangesResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        _logger.LogInformation("File {FileId} saved", request.FileId);

        var response = new CompleteMultipartUploadResponse { FileId = request.FileId };

        return response;
    }
}
