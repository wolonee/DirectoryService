using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.SimpleUpload;

public sealed record CancelUploadCommand(Guid FileId) : ICommand;

public sealed class CancelUploadValidator : AbstractValidator<CancelUploadCommand>
{
    public CancelUploadValidator()
    {
        RuleFor(command => command.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CancelUploadCommand.FileId)));
    }
}

public sealed class CancelUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/{fileId:guid}/cancel", async Task<EndpointResult<CancelUploadResponse>> (
            [FromRoute] Guid fileId,
            [FromServices] ICommandHandler<CancelUploadResponse, CancelUploadCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CancelUploadCommand(fileId);
            
            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class CancelUploadHandler
    : ICommandHandler<CancelUploadResponse, CancelUploadCommand>
{
    private readonly IMediaAssetRepository _repository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<CancelUploadCommand> _validator;
    private readonly ILogger<CancelUploadHandler> _logger;

    public CancelUploadHandler(
        IMediaAssetRepository repository,
        IS3Provider s3Provider,
        IValidator<CancelUploadCommand> validator,
        ILogger<CancelUploadHandler> logger)
    {
        _repository = repository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CancelUploadResponse, Errors>> Handle(
        CancelUploadCommand command,
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
        
        if (asset.Status != MediaStatus.UPLOADING)
        {
            _logger.LogWarning(
                "Cannot cancel media asset {FileId} from status {Status}",
                asset.Id,
                asset.Status);
            return MediaAssetErrors.InvalidStatus(asset.Id, asset.Status).ToErrors();
        }

        var deleteResult = await _s3Provider.DeleteObjectAsync(asset.RawKey, cancellationToken);
        if (deleteResult.IsFailure)
        {
            _logger.LogError("Media was not deleted");
            return deleteResult.Error.ToErrors();
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
        
        var response = new CancelUploadResponse
        {
            FileId = asset.Id,
            Status = asset.Status.ToString().ToLowerInvariant(),
        };
        
        return response;
    }

}
