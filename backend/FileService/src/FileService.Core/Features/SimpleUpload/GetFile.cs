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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.SimpleUpload;

public sealed record GetFileQuery(Guid FileId);

public sealed class GetFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{fileId:guid}", async Task<EndpointResult<FileResponse>> (
            [FromRoute] Guid fileId,
            [FromServices] GetFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.Handle(new GetFileQuery(fileId), cancellationToken);
        });
    }
}

public sealed class GetFileHandler
{
    private readonly IMediaAssetRepository _repository;
    private readonly IS3Provider _s3Provider;

    public GetFileHandler(IMediaAssetRepository repository, IS3Provider s3Provider)
    {
        _repository = repository;
        _s3Provider = s3Provider;
    }

    public async Task<Result<FileResponse, Error>> Handle(
        GetFileQuery query,
        CancellationToken cancellationToken)
    {
        if (query.FileId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(query.FileId));

        Result<MediaAsset, Error> assetResult =
            await _repository.GetByIdAsync(query.FileId, cancellationToken);

        if (assetResult.IsFailure)
            return assetResult.Error;

        MediaAsset asset = assetResult.Value;

        if (asset.Status == MediaStatus.DELETED)
            return GeneralErrors.NotFound(query.FileId, "File");

        ObjectMetadataDto? storage = asset.StorageReference is null
            ? null
            : new ObjectMetadataDto(
                asset.StorageReference.Size,
                asset.StorageReference.ContentType,
                asset.StorageReference.ETag,
                asset.StorageReference.Checksum,
                asset.StorageReference.LastModified);

        if (asset.Status != MediaStatus.READY)
        {
            return new FileResponse(
                asset.Id,
                asset.Status.ToString(),
                asset.MediaData.FileName.Name,
                asset.MediaData.ContentType.Value,
                asset.MediaData.Size,
                storage,
                null);
        }

        if (asset.StorageReference is null || asset.FinalKey == StorageKey.None)
            return Error.Conflict("media-asset.storage-reference-missing", "Ready file has no storage reference.");

        Result<string, Error> urlResult =
            await _s3Provider.GenerateDownloadUrlAsync(asset.FinalKey);

        if (urlResult.IsFailure)
            return urlResult.Error;

        return new FileResponse(
            asset.Id,
            asset.Status.ToString(),
            asset.MediaData.FileName.Name,
            asset.MediaData.ContentType.Value,
            asset.MediaData.Size,
            storage,
            urlResult.Value);
    }
}
