using CSharpFunctionalExtensions;
using DirectoryService.Presentation.EndpointResults;
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

public record AbortMultipartUploadCommand(AbortMultipartUploadRequest Request);

public sealed class AbortMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/abort", async Task<EndpointResult<AbortMultipartUploadResponse>>(
            [FromBody] AbortMultipartUploadRequest request,
            [FromServices] AbortMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AbortMultipartUploadCommand(request);

            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class AbortMultipartUploadHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;

    public AbortMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetRepository mediaAssetRepository,
        ICurrentUser currentUser,
        ILogger<AbortMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaAssetRepository = mediaAssetRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<AbortMultipartUploadResponse, Error>> Handle(
        AbortMultipartUploadCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        var mediaAssetResult = await _mediaAssetRepository.GetByIdAsync(request.FileId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var asset = mediaAssetResult.Value;
        
        if (asset.Owner.UploaderId != _currentUser.UserId)
            return MediaAssetErrors.WrongUploader(request.FileId);

        if (asset.Status == MediaStatus.DELETED)
            return MediaAssetErrors.AlreadyDeleted(asset.Id);
        
        if (asset.Status != MediaStatus.UPLOADING)
            return MediaAssetErrors.InvalidStatus(asset.Id, asset.Status);

        if (asset.MultipartUploadId != request.UploadId)
            return MediaAssetErrors.InvalidMultipartUploadId(asset.Id);
        
        var deleteResult = await _s3Provider.AbortMultipartUploadAsync(asset.RawKey, asset.MultipartUploadId, cancellationToken);
        if (deleteResult.IsFailure)
            return deleteResult.Error;
        
        var markDeletedResult = asset.MarkDeleted(DateTime.UtcNow);
        if (markDeletedResult.IsFailure)
            return markDeletedResult.Error;
        
        var saveChangesResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;
        
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
