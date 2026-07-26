using CSharpFunctionalExtensions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.Errors;
using FileService.Core;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Contracts;

public record DeleteFileCommand(Guid FileId);

public sealed class DeleteMediaAssetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/files/{fileId:guid}",
            async Task<EndpointResult<DeleteMediaAssetResponse>> (
                [FromRoute] Guid fileId,
                [FromServices] DeleteMediaAssetHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteFileCommand(fileId);

                return await handler.Handle(command, cancellationToken);
            });
    }
}

public sealed class DeleteMediaAssetHandler
{
    private readonly IMediaAssetRepository _repository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<DeleteMediaAssetHandler> _logger;

    public DeleteMediaAssetHandler(
        IMediaAssetRepository repository,
        IS3Provider s3Provider,
        ILogger<DeleteMediaAssetHandler> logger)
    {
        _repository = repository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<DeleteMediaAssetResponse, Error>> Handle(
        DeleteFileCommand command,
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
        
        if (asset.Status != MediaStatus.DELETED)
        {
            _logger.LogError("Media asset not found");
            return MediaAssetErrors.AlreadyCompleted(asset.Id);
        }

        var deleteResult = await _s3Provider.DeleteObjectAsync(asset.FinalKey, cancellationToken);
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
        
        var response = new DeleteMediaAssetResponse
        {
            FileId = asset.Id,
            Status = asset.Status.ToString().ToLowerInvariant(),
        };
        
        return response;
    }
}