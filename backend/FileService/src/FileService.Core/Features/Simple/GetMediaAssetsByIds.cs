using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Core.Caching;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Infrastructure.S3;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Core.Features.SimpleUpload;

public sealed record GetMediaAssetsQuery(IEnumerable<Guid> FileIds) : IQuery;

public sealed class GetMediaAssetsValidator : AbstractValidator<GetMediaAssetsQuery>
{
    public GetMediaAssetsValidator()
    {
        RuleFor(query => query.FileIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetMediaAssetsQuery.FileIds)));
    }
}

public sealed class GetMediaAssetsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files", async Task<EndpointResult<GetMediaAssetsResponse>> (
            [FromBody] GetMediaAssetsRequest request,
            [FromServices] IQueryHandler<GetMediaAssetsResponse, GetMediaAssetsQuery> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMediaAssetsQuery(request.FileIds);

            return await handler.Handle(query, cancellationToken);
        });
    }
}

public sealed class GetMediaAssetsHandler
    : IQueryHandler<GetMediaAssetsResponse, GetMediaAssetsQuery>
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;
    private readonly HybridCache _cache;
    private readonly ILogger<GetMediaAssetsHandler> _logger;
    private readonly IValidator<GetMediaAssetsQuery> _validator;

    public GetMediaAssetsHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext,
        HybridCache cache,
        ILogger<GetMediaAssetsHandler> logger,
        IValidator<GetMediaAssetsQuery> validator)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
        _cache = cache;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<GetMediaAssetsResponse, Errors>> Handle(GetMediaAssetsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        var fileIds = query.FileIds;

        if (!fileIds.Any())
            return new GetMediaAssetsResponse([]);

        List<MediaAsset> mediaAssets = await _readDbContext.MediaAssetsQuery
            .Where(x => fileIds.Contains(x.Id) && x.Status != MediaStatus.DELETED)
            .ToListAsync(cancellationToken: cancellationToken);

        List<MediaAsset> readyMediaAssets = mediaAssets.Where(x => x.Status == MediaStatus.READY).ToList();
        List<StorageKey> keys = readyMediaAssets.Select(x => x.FinalKey).ToList();

        Dictionary<StorageKey, string?> urlsDict = await GetPresignedUrlsFromCacheAsync(keys, cancellationToken);

        var response = new List<GetMediaAssetDto>();
        foreach (var mediaAsset in mediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.FinalKey, out string? url);

            var res = new GetMediaAssetDto(
                mediaAsset.Id,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                mediaAsset.MediaData.ContentType.Value.ToLowerInvariant(),
                url);
            
            response.Add(res);
        }

        return new GetMediaAssetsResponse(response.ToArray());
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













