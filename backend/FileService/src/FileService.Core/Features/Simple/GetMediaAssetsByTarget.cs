using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.Simple.GetMediaAssetsByTarget;
using FileService.Core.Abstractions;
using FileService.Core.Caching;
using FileService.Core.Models;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.Simple;

public sealed record GetMediaAssetsByTargetQuery(GetMediaAssetsByTargetRequest Request) : IQuery;

public sealed class GetMediaAssetsByTargetValidator : AbstractValidator<GetMediaAssetsByTargetQuery>
{
    public GetMediaAssetsByTargetValidator()
    {
        RuleFor(query => query.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetMediaAssetsByTargetQuery.Request)));

        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.TargetId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(GetMediaAssetsByTargetRequest.TargetId)));
        });
    }
}

public sealed class GetMediaAssetsByTargetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files", async Task<EndpointResult<GetMediaAssetsByTargetResponse>>(
            [AsParameters] GetMediaAssetsByTargetRequest request,
            [FromServices] IQueryHandler<GetMediaAssetsByTargetResponse, GetMediaAssetsByTargetQuery> handler,
            CancellationToken cancellationToken) => 
        {
            var query = new GetMediaAssetsByTargetQuery(request);
            
            return await handler.Handle(query, cancellationToken);
        });
    }
}

public sealed class GetMediaAssetsByTargetHandler
    : IQueryHandler<GetMediaAssetsByTargetResponse, GetMediaAssetsByTargetQuery>
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;
    private readonly HybridCache _cache;
    private readonly ILogger<GetMediaAssetsByTargetHandler> _logger;
    private readonly IValidator<GetMediaAssetsByTargetQuery> _validator;

    public GetMediaAssetsByTargetHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext,
        HybridCache cache,
        ILogger<GetMediaAssetsByTargetHandler> logger,
        IValidator<GetMediaAssetsByTargetQuery> validator)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
        _cache = cache;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<GetMediaAssetsByTargetResponse, Errors>> Handle(
        GetMediaAssetsByTargetQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        var request = query.Request;

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

        Dictionary<StorageKey, string?> urlsDict = await GetPresignedUrlsFromCacheAsync(keys, cancellationToken);

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

    private async Task<Dictionary<StorageKey, string?>> GetPresignedUrlsFromCacheAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken)
    {
        var keys = storageKeys.ToList();

        if (!keys.Any())
            return [];

        IEnumerable<Task<(StorageKey key, string? url)>> cacheUrlsTasks = keys.Select(async key =>
        {
            string? url = await _cache.GetOrCreateAsync(
                key: MediaAssetCacheKeys.DownloadUrl(key),
                factory: _ => ValueTask.FromResult<string?>(null),
                cancellationToken: cancellationToken);

            return (key, url);
        });

        (StorageKey key, string? url)[] cacheUrls = await Task.WhenAll(cacheUrlsTasks);

        var result = new Dictionary<StorageKey, string?>();

        var keysToGenerate = new List<StorageKey>();
        foreach (var (key, url) in cacheUrls)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                keysToGenerate.Add(key);
            }
            else
            {
                result[key] = url;
            }
        }

        if (keysToGenerate.Any())
        {
            Result<MediaUrl[], Error> mediaUrlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keysToGenerate, cancellationToken);
            if (mediaUrlsResult.IsFailure)
            {
                _logger.LogWarning("Не удалось подписать {Count} ссылок: {Error}", keysToGenerate.Count, mediaUrlsResult.Error);
                return result;
            }

            IEnumerable<Task> setTasks = mediaUrlsResult.Value.Select(async mediaUrl =>
            {
                result[mediaUrl.StorageKey] = mediaUrl.PresignedUrl;

                await _cache.SetAsync(
                    key: MediaAssetCacheKeys.DownloadUrl(mediaUrl.StorageKey),
                    value: mediaUrl.PresignedUrl,
                    cancellationToken: cancellationToken);
            });

            await Task.WhenAll(setTasks);
        }

        return result;
    }
}











