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

namespace FileService.Core.Features.SimpleUpload;

public sealed record CancelUploadCommand(Guid FileId);

public sealed class CancelUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/{fileId:guid}", async Task<EndpointResult<CancelUploadResponse>> (
            [FromRoute] Guid fileId,
            [FromServices] CancelUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CancelUploadCommand(fileId);
            
            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class CancelUploadHandler
{
    private readonly IMediaAssetRepository _repository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<CancelUploadHandler> _logger;

    public CancelUploadHandler(
        IMediaAssetRepository repository,
        IS3Provider s3Provider,
        ILogger<CancelUploadHandler> logger)
    {
        _repository = repository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<CancelUploadResponse, Error>> Handle(
        CancelUploadCommand command,
        CancellationToken cancellationToken)
    {
        var fileId = command.FileId;
        
        var assetResult = await _repository.GetByIdAsync(fileId, cancellationToken);
        if (assetResult.IsFailure)
        {
            _logger.LogError("Media asset not found");
            return assetResult.Error;
        }
        
        var asset = assetResult.Value;
        
        if (asset.Status != MediaStatus.UPLOADING)
        {
            _logger.LogError("Media asset not found");
            return MediaAssetErrors.AlreadyCompleted(asset.Id);
        }

        var deleteResult = await _s3Provider.DeleteObjectAsync(asset.RawKey, cancellationToken);
        if (deleteResult.IsFailure)
        {
            _logger.LogError("Media was not deleted");
            return deleteResult.Error;
        }
        
        var markDeleted = asset.MarkDeleted(DateTime.UtcNow);
        if (markDeleted.IsFailure)
        {
            _logger.LogError("Media was not marked as deleted");
            return markDeleted.Error;
        }

        var saveChanges = await _repository.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            _logger.LogError("Save changes failed");
            return saveChanges.Error;
        }
        
        var response = new CancelUploadResponse
        {
            FileId = asset.Id,
            Status = asset.Status.ToString().ToLowerInvariant(),
        };
        
        return response;
    }
}