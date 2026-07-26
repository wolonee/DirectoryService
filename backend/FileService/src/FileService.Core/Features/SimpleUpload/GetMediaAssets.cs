using CSharpFunctionalExtensions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FileService.Core.Features.SimpleUpload;

public sealed record GetMediaAssetsQuery(IEnumerable<Guid> FileIds);

public sealed class GetMediaAssetsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files", async Task<EndpointResult<GetMediaAssetsResponse>> (
            [FromBody] GetMediaAssetsQuery query,
            [FromServices] GetMediaAssetsHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.Handle(query, cancellationToken);
        });
    }
}

public sealed class GetMediaAssetsHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;

    public GetMediaAssetsHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetMediaAssetsResponse, Error>> Handle(GetMediaAssetsQuery query, CancellationToken cancellationToken)
    {
        var fileIds = query.FileIds;

        if (!fileIds.Any())
            return new GetMediaAssetsResponse([]);

        List<MediaAsset> mediaAssets = await _readDbContext.MediaAssetsQuery
            .Where(x => fileIds.Contains(x.Id) && x.Status != MediaStatus.DELETED)
            .ToListAsync(cancellationToken: cancellationToken);

        List<MediaAsset> readyMediaAssets = mediaAssets.Where(x => x.Status == MediaStatus.READY).ToList();
        List<StorageKey> keys = readyMediaAssets.Select(x => x.FinalKey).ToList();
        
        Result<MediaUrl[], Error> urlResult = await _s3Provider.GenerateDownloadUrlsAsync(keys, cancellationToken);
        if (urlResult.IsFailure)
            return urlResult.Error;
        
        var urls = urlResult.Value;
        
        var urlsDict = urls.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);

        var response = new List<GetMediaAssetDto>();
        foreach (var mediaAsset in mediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.FinalKey, out string? url);

            var res = new GetMediaAssetDto(
                mediaAsset.Id,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                mediaAsset.MediaData.ContentType.ToString().ToLowerInvariant(),
                url);
            
            response.Add(res);
        }

        return new GetMediaAssetsResponse(response.ToArray());
    }
}
