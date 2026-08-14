using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.MultipartUpload.StartMultipartUpload;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.MultipartUpload;

public sealed record StartMultipartUploadCommand(StartMultipartUploadRequest Request) : ICommand;

public sealed class StartMultipartUploadValidator : AbstractValidator<StartMultipartUploadCommand>
{
    public StartMultipartUploadValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(StartMultipartUploadCommand.Request)));

        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.FileName)
                .MustBeValueObject(FileName.Create);

            RuleFor(command => command.Request.ContentType)
                .MustBeValueObject(ContentType.Create);

            RuleFor(command => command.Request.Size)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid(nameof(StartMultipartUploadRequest.Size)));

            RuleFor(command => command.Request.AssetType)
                .MustBeValueObject(assetType => assetType.ToAssetType());

            RuleFor(command => command.Request.Usage)
                .MustBeValueObject(usage => usage.ToMediaUsage());

            RuleFor(command => command.Request.TargetType)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(StartMultipartUploadRequest.TargetType)));

            RuleFor(command => command.Request.TargetId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(StartMultipartUploadRequest.TargetId)));
        });
    }
}

public sealed class StartMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/start", async Task<EndpointResult<StartMultipartUploadResponse>>(
            [FromBody] StartMultipartUploadRequest request, 
            [FromServices] ICommandHandler<StartMultipartUploadResponse, StartMultipartUploadCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new StartMultipartUploadCommand(request);
            
            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class StartMultipartUploadHandler
    : ICommandHandler<StartMultipartUploadResponse, StartMultipartUploadCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IMediaAssetFactory _mediaAssetFactory;
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly IValidator<StartMultipartUploadCommand> _validator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<StartMultipartUploadHandler> _logger;

    public StartMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetRepository mediaAssetRepository,
        ITransactionManager transactionManager,
        IMediaAssetFactory mediaAssetFactory,
        IChunkSizeCalculator chunkSizeCalculator,
        IValidator<StartMultipartUploadCommand> validator,
        ICurrentUser currentUser,
        ILogger<StartMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaAssetRepository = mediaAssetRepository;
        _transactionManager = transactionManager;
        _mediaAssetFactory = mediaAssetFactory;
        _chunkSizeCalculator = chunkSizeCalculator;
        _validator = validator;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<StartMultipartUploadResponse, Errors>> Handle(StartMultipartUploadCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();
        
        var request = command.Request;
        
        var fileName = FileName.Create(request.FileName).Value;
        var contentType = ContentType.Create(request.ContentType).Value;
        
        Result<(long ChunkSize, int TotalChunks), Error> chunksDataResult = _chunkSizeCalculator.CalculateChunkSize(request.Size);
        if (chunksDataResult.IsFailure)
            return chunksDataResult.Error.ToErrors();
        
        (long chunkSize, int totalChunks) = chunksDataResult.Value;

        var mediaDataResult = MediaData.Create(fileName, contentType, request.Size, totalChunks);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error.ToErrors();

        var ownerResult = MediaOwner.Create(request.TargetType, request.TargetId, _currentUser.UserId);
        if (ownerResult.IsFailure)
            return ownerResult.Error.ToErrors();
        
        var mediaData = mediaDataResult.Value;
        var owner = ownerResult.Value;
        var assetType = request.AssetType.ToAssetType().Value;
        var usage = request.Usage.ToMediaUsage().Value;

        Guid id = Guid.CreateVersion7();
        var mediaAssetResult = _mediaAssetFactory.CreateForUpload(id, assetType, mediaData, usage, owner);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();
        
        var mediaAsset = mediaAssetResult.Value;

        Result<Guid, Error> addResult = _mediaAssetRepository.Add(mediaAsset);
        if (addResult.IsFailure)
        {
            _logger.LogError("Saving multipart media asset {MediaAssetId} failed", mediaAsset.Id);
            return addResult.Error.ToErrors();
        }

        Result<string, Error> uploadIdResult = await _s3Provider.StartMultipartUploadAsync(mediaAsset.UploadKey, contentType, cancellationToken);
        if (uploadIdResult.IsFailure)
        {
            _logger.LogError("Starting multipart upload failed for media asset {MediaAssetId}", mediaAsset.Id);
            return uploadIdResult.Error.ToErrors();
        }

        var setUploadIdResult = mediaAsset.SetMultipartUploadId(uploadIdResult.Value);
        if (setUploadIdResult.IsFailure)
            return setUploadIdResult.Error.ToErrors();

        var saveUploadIdResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveUploadIdResult.IsFailure)
            return saveUploadIdResult.Error.ToErrors();
        
        Result<IReadOnlyList<MultipartPartUploadDto>, Error> generateAllChunksResult = await _s3Provider.GenerateAllChunksUploadUrlsAsync(mediaAsset.UploadKey, uploadIdResult.Value, totalChunks, cancellationToken);
        if (generateAllChunksResult.IsFailure)
        {
            _logger.LogError("Generating multipart part URLs failed for media asset {MediaAssetId}", mediaAsset.Id);
            return generateAllChunksResult.Error.ToErrors();
        }

        _logger.LogInformation(
            "Multipart upload initiated for media asset {MediaAssetId} with {TotalChunks} parts",
            mediaAsset.Id,
            totalChunks);

        var response = new StartMultipartUploadResponse
        {
            FileId = mediaAsset.Id,
            UploadId = uploadIdResult.Value,
            ChunkSize = chunkSize,
            TotalChunks = totalChunks,
            Parts = generateAllChunksResult.Value,
        };
        
        return response;
    }

}
