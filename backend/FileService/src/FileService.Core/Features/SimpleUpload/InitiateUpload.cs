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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.SimpleUpload;

public sealed class InitiateUploadValidator : AbstractValidator<InitiateUploadCommand>
{
    public InitiateUploadValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(InitiateUploadCommand.Request)));

        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.FileName)
                .MustBeValueObject(FileName.Create);

            RuleFor(command => command.Request.ContentType)
                .MustBeValueObject(ContentType.Create);

            RuleFor(command => command.Request.Size)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid(nameof(InitiateUploadRequest.Size)));

            RuleFor(command => command.Request.AssetType)
                .MustBeValueObject(assetType => assetType.ToAssetType());

            RuleFor(command => command.Request.Usage)
                .MustBeValueObject(usage => usage.ToMediaUsage());

            RuleFor(command => command.Request.TargetType)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(InitiateUploadRequest.TargetType)));

            RuleFor(command => command.Request.TargetId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(InitiateUploadRequest.TargetId)));
        });
    }
}

public sealed class InitiateUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/initiate", async Task<EndpointResult<InitiateUploadResponse>> (
            [FromBody] InitiateUploadRequest request,
            [FromServices] InitiateUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new InitiateUploadCommand(request);
            return await handler.Handle(command, cancellationToken);
        });
    }
}


public sealed class InitiateUploadHandler
    : ICommandHandler<InitiateUploadResponse, InitiateUploadCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IMediaAssetFactory _mediaAssetFactory;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<InitiateUploadCommand> _validator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<InitiateUploadHandler> _logger;

    public InitiateUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        IMediaAssetFactory mediaAssetFactory,
        IS3Provider s3Provider,
        IValidator<InitiateUploadCommand> validator,
        ICurrentUser currentUser,
        ILogger<InitiateUploadHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _mediaAssetFactory = mediaAssetFactory;
        _s3Provider = s3Provider;
        _validator = validator;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<InitiateUploadResponse, Errors>> Handle(
        InitiateUploadCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        InitiateUploadRequest request = command.Request;

        var fileName = FileName.Create(request.FileName).Value;
        var contentType = ContentType.Create(request.ContentType).Value;
        var assetType = request.AssetType.ToAssetType().Value;
        var usage = request.Usage.ToMediaUsage().Value;

        if (_currentUser.UserId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("UploaderId").ToErrors();

        Result<MediaOwner, Error> mediaOwnerResult = MediaOwner.Create(
            request.TargetType,
            request.TargetId,
            _currentUser.UserId);
        if (mediaOwnerResult.IsFailure)
            return mediaOwnerResult.Error.ToErrors();

        Result<MediaData, Error> mediaDataResult = MediaData.Create(
            fileName,
            contentType,
            request.Size);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error.ToErrors();

        Guid mediaAssetId = Guid.CreateVersion7();
        Result<MediaAsset, Error> mediaAssetResult =
            _mediaAssetFactory.CreateForUpload(
                mediaAssetId,
                assetType,
                mediaDataResult.Value,
                usage,
                mediaOwnerResult.Value);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        Result<PresignedUploadDto, Error> uploadUrlResult =
            await _s3Provider.GenerateUploadUrlAsync(
                mediaAssetResult.Value.RawKey,
                mediaAssetResult.Value.MediaData.ContentType,
                cancellationToken);
        if (uploadUrlResult.IsFailure)
            return uploadUrlResult.Error.ToErrors();

        await _mediaAssetRepository.AddAsync(mediaAssetResult.Value, cancellationToken);

        _logger.LogInformation("Initiated upload for media asset {MediaAssetId}", mediaAssetId);

        return new InitiateUploadResponse
        {
            FileId = mediaAssetId,
            Upload = uploadUrlResult.Value,
        };
    }

}
