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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.SimpleUpload;

public sealed record InitiateUploadCommand(InitiateUploadRequest Request);

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
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IMediaAssetFactory _mediaAssetFactory;
    private readonly IS3Provider _s3Provider;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<InitiateUploadHandler> _logger;

    public InitiateUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        IMediaAssetFactory mediaAssetFactory,
        IS3Provider s3Provider,
        ICurrentUser currentUser,
        ILogger<InitiateUploadHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _mediaAssetFactory = mediaAssetFactory;
        _s3Provider = s3Provider;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<InitiateUploadResponse, Error>> Handle(
        InitiateUploadCommand command,
        CancellationToken cancellationToken)
    {
        InitiateUploadRequest request = command.Request;

        Result<FileName, Error> fileNameResult = FileName.Create(request.FileName);
        if (fileNameResult.IsFailure)
            return fileNameResult.Error;

        Result<ContentType, Error> contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error;

        Result<AssetType, Error> assetTypeResult = request.AssetType.ToAssetType();
        if (assetTypeResult.IsFailure)
            return assetTypeResult.Error;

        Result<MediaUsage, Error> usageResult = request.Usage.ToMediaUsage();
        if (usageResult.IsFailure)
            return usageResult.Error;

        if (_currentUser.UserId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("UploaderId");

        Result<MediaOwner, Error> mediaOwnerResult = MediaOwner.Create(
            request.TargetType,
            request.TargetId,
            _currentUser.UserId);
        if (mediaOwnerResult.IsFailure)
            return mediaOwnerResult.Error;

        Result<MediaData, Error> mediaDataResult = MediaData.Create(
            fileNameResult.Value,
            contentTypeResult.Value,
            request.Size);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error;

        Guid mediaAssetId = Guid.CreateVersion7();
        Result<Domain.Assets.MediaAsset, Error> mediaAssetResult =
            _mediaAssetFactory.CreateForUpload(
                mediaAssetId,
                assetTypeResult.Value,
                mediaDataResult.Value,
                usageResult.Value,
                mediaOwnerResult.Value);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        Result<PresignedUploadDto, Error> uploadUrlResult =
            await _s3Provider.GenerateUploadUrlAsync(
                mediaAssetResult.Value.RawKey,
                mediaAssetResult.Value.MediaData.ContentType,
                cancellationToken);
        if (uploadUrlResult.IsFailure)
            return uploadUrlResult.Error;

        await _mediaAssetRepository.AddAsync(mediaAssetResult.Value, cancellationToken);

        _logger.LogInformation("Initiated upload for media asset {MediaAssetId}", mediaAssetId);

        return new InitiateUploadResponse
        {
            FileId = mediaAssetId,
            Upload = uploadUrlResult.Value,
        };
    }
}
