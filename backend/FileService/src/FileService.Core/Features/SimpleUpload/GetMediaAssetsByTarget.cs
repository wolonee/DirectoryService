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

public record GetMediaAssetsByTargetQuery(GetMediaAssetsByTargetRequest request);

public sealed class GetMediaAssetsByTargetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files?targetType={type}&targetId={id}", async Task<EndpointResult<GetMediaAssetsByTargetResponse>>(
            [FromRoute] GetMediaAssetsByTargetRequest request,
            [FromServices] GetMediaAssetsByTargetHandler handler,
            CancellationToken cancellationToken) => 
        {
            var query = new GetMediaAssetsByTargetQuery(request);
            
            return await handler.Handle(query, cancellationToken);
        });
    }
}

public sealed class GetMediaAssetsByTargetHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;

    public GetMediaAssetsByTargetHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetMediaAssetsByTargetResponse, Error>> Handle(
        GetMediaAssetsByTargetQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.request;

        IQueryable<MediaAsset> mediaAssetsQuery = _readDbContext.MediaAssetsQuery
            .Where(asset => 
                asset.Owner.EntityId == request.TargetId
                && asset.Status != MediaStatus.DELETED);

        if (!string.IsNullOrWhiteSpace(request.TargetType))
            mediaAssetsQuery = mediaAssetsQuery.Where(asset => asset.Owner.Context == request.TargetType);

        List<MediaAsset> mediaAssets = await mediaAssetsQuery
            .ToListAsync(cancellationToken);
        
        List<MediaAsset> readyMediaAssets = mediaAssets.Where(a => a.Status == MediaStatus.READY).ToList();
        List<StorageKey> keys = readyMediaAssets.Select(a => a.FinalKey).ToList();

        var urlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keys, cancellationToken);
        if (urlsResult.IsFailure)
            return urlsResult.Error;
        
        var urls = urlsResult.Value;
        
        var urlsDict = urls.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);

        var mediaAssetDtoList = new List<GetMediaAssetByTargetDto>();
        foreach (var mediaAsset in mediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.FinalKey, out string? url);

            var dto = new GetMediaAssetByTargetDto(
                mediaAsset.Id,
                mediaAsset.Owner.EntityId,
                mediaAsset.Owner.Context,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                mediaAsset.MediaData.ContentType.Value.ToLowerInvariant(),
                url);
            
            mediaAssetDtoList.Add(dto);
        }
        
        var response = new GetMediaAssetsByTargetResponse
        {
            TargetId = request.TargetId,
            TargetType = request.TargetType,
            Files = mediaAssetDtoList,
        };

        return response;
    }
}











