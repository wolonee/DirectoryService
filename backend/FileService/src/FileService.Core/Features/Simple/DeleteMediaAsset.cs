using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.SimpleUpload;

public sealed record DeleteFileCommand(Guid FileId) : ICommand;

public sealed class DeleteMediaAssetValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteMediaAssetValidator()
    {
        RuleFor(command => command.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(DeleteFileCommand.FileId)));
    }
}

public sealed class DeleteMediaAssetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/files/{fileId:guid}",
            async Task<EndpointResult<DeleteMediaAssetResponse>> (
                [FromRoute] Guid fileId,
                [FromServices] ICommandHandler<DeleteMediaAssetResponse, DeleteFileCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteFileCommand(fileId);

                return await handler.Handle(command, cancellationToken);
            });
    }
}

public sealed class DeleteMediaAssetHandler
    : ICommandHandler<DeleteMediaAssetResponse, DeleteFileCommand>
{
    private readonly IMediaAssetRepository _repository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<DeleteFileCommand> _validator;
    private readonly ILogger<DeleteMediaAssetHandler> _logger;

    public DeleteMediaAssetHandler(
        IMediaAssetRepository repository,
        IS3Provider s3Provider,
        IValidator<DeleteFileCommand> validator,
        ILogger<DeleteMediaAssetHandler> logger)
    {
        _repository = repository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DeleteMediaAssetResponse, Errors>> Handle(
        DeleteFileCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        var fileId = command.FileId;
        
        var assetResult = await _repository.GetByIdAsync(fileId, cancellationToken);
        if (assetResult.IsFailure)
        {
            _logger.LogError("Media asset not found");
            return assetResult.Error.ToErrors();
        }
        
        var asset = assetResult.Value;
        
        if (asset.Status != MediaStatus.READY)
        {
            _logger.LogWarning(
                "Cannot delete media asset {FileId} from status {Status}",
                asset.Id,
                asset.Status);
            return MediaAssetErrors.InvalidStatus(asset.Id, asset.Status).ToErrors();
        }
        
        var keysToDelete = new HashSet<StorageKey>
        {
            asset.FinalKey,
            asset.RawKey,
        };

        foreach (var key in keysToDelete)
        {
            var deleteResult = await _s3Provider.DeleteObjectAsync(key, cancellationToken);
            if (deleteResult.IsFailure)
            {
                _logger.LogError("Media was not deleted");
                return deleteResult.Error.ToErrors();
            }
        }
        
        var markDeleted = asset.MarkDeleted(DateTime.UtcNow);
        if (markDeleted.IsFailure)
        {
            _logger.LogError("Media was not marked as deleted");
            return markDeleted.Error.ToErrors();
        }

        var saveChanges = await _repository.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            _logger.LogError("Save changes failed");
            return saveChanges.Error.ToErrors();
        }
        
        var response = new DeleteMediaAssetResponse
        {
            FileId = asset.Id,
            Status = asset.Status.ToString().ToLowerInvariant(),
        };
        
        return response;
    }

}
