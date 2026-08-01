using CSharpFunctionalExtensions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Infrastructure.S3;
using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features;

public record StartMultipartUploadCommand(StartMultipartUploadRequest Request);

public sealed class StartMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/start", async Task<EndpointResult<StartMultipartUploadResponse>>(
            [FromBody] StartMultipartUploadRequest request, 
            [FromServices] StartMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new StartMultipartUploadCommand(request);
            
            return await handler.Handle(command, cancellationToken);
        });
    }
}

public sealed class StartMultipartUploadHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IMediaAssetFactory _mediaAssetFactory;
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<StartMultipartUploadHandler> _logger;

    public StartMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetRepository mediaAssetRepository,
        IMediaAssetFactory mediaAssetFactory,
        IChunkSizeCalculator chunkSizeCalculator,
        ICurrentUser currentUser,
        ILogger<StartMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaAssetRepository = mediaAssetRepository;
        _mediaAssetFactory = mediaAssetFactory;
        _chunkSizeCalculator = chunkSizeCalculator;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<StartMultipartUploadResponse, Error>> Handle(StartMultipartUploadCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        var fileNameResult = FileName.Create(request.FileName);
        if (fileNameResult.IsFailure)
            return fileNameResult.Error;
        
        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error;
        
        Result<(long ChunkSize, int TotalChunks), Error> chunksDataResult = _chunkSizeCalculator.CalculateChunkSize(request.Size);
        if (chunksDataResult.IsFailure)
            return chunksDataResult.Error;
        
        (long chunkSize, int totalChunks) = chunksDataResult.Value;
        var fileName = fileNameResult.Value;
        var contentType = contentTypeResult.Value;

        var mediaDataResult = MediaData.Create(fileName, contentType, request.Size, totalChunks);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error;

        var ownerResult = MediaOwner.Create(request.TargetType, request.TargetId, _currentUser.UserId);
        if (ownerResult.IsFailure)
            return ownerResult.Error;
        
        var assetTypeResult = request.AssetType.ToAssetType();
        if (assetTypeResult.IsFailure)
            return assetTypeResult.Error;

        var usageResult = request.Usage.ToMediaUsage();
        if (usageResult.IsFailure)
            return usageResult.Error;
        
        var mediaData = mediaDataResult.Value;
        var owner = ownerResult.Value;
        var assetType = assetTypeResult.Value;
        var usage = usageResult.Value;

        Guid id = Guid.CreateVersion7();
        var mediaAssetResult = _mediaAssetFactory.CreateForUpload(id, assetType, mediaData, usage, owner);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;
        
        var mediaAsset = mediaAssetResult.Value;

        Result<Guid, Error> addResult = await _mediaAssetRepository.AddAsync(mediaAsset, cancellationToken);
        if (addResult.IsFailure)
        {
            _logger.LogError("Saving multipart media asset {MediaAssetId} failed", mediaAsset.Id);
            return addResult.Error;
        }

        Result<string, Error> uploadIdResult = await _s3Provider.StartMultipartUploadAsync(mediaAsset.RawKey, contentType, cancellationToken);
        if (uploadIdResult.IsFailure)
        {
            _logger.LogError("Starting multipart upload failed for media asset {MediaAssetId}", mediaAsset.Id);
            return uploadIdResult.Error;
        }

        var setUploadIdResult = mediaAsset.SetMultipartUploadId(uploadIdResult.Value);
        if (setUploadIdResult.IsFailure)
            return setUploadIdResult.Error;

        var saveUploadIdResult = await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
        if (saveUploadIdResult.IsFailure)
            return saveUploadIdResult.Error;
        
        Result<IReadOnlyList<MultipartPartUploadDto>, Error> generateAllChunksResult = await _s3Provider.GenerateAllChunksUploadUrlsAsync(mediaAsset.RawKey, uploadIdResult.Value, totalChunks, cancellationToken);
        if (generateAllChunksResult.IsFailure)
        {
            _logger.LogError("Generating multipart part URLs failed for media asset {MediaAssetId}", mediaAsset.Id);
            return generateAllChunksResult.Error;
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
