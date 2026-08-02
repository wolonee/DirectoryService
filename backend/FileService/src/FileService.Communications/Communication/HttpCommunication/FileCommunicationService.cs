using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using FileService.Contracts.Features.AssetExists;
using Microsoft.Extensions.Logging;

namespace FileService.Contracts.HttpCommunication;

internal sealed class FileCommunicationService : IFileCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileCommunicationService> _logger;

    public FileCommunicationService(
        HttpClient httpClient,
        ILogger<FileCommunicationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<GetMediaAssetResponse, Errors>> GetMediaAsset(
        GetMediaAssetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"/files/{request.FileId}",
                cancellationToken);

            return await response.HandleResponseAsync<GetMediaAssetResponse>(cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Network failure while getting media asset {FileId} from File Service",
                request.FileId);

            return FileServiceClientErrors.Unavailable().ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssetsByIds(
        GetMediaAssetsRequest request,
        CancellationToken cancellationToken)
    {
        int fileCount = request.FileIds.Count();

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                "/files",
                request,
                cancellationToken);

            return await response.HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Network failure while getting {FileCount} media assets from File Service",
                fileCount);

            return FileServiceClientErrors.Unavailable().ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetsByTargetResponse, Errors>> GetMediaAssetsByTarget(
        GetMediaAssetsByTargetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            string requestUri = $"/files?TargetId={request.TargetId}";

            if (!string.IsNullOrWhiteSpace(request.TargetType))
                requestUri += $"&TargetType={Uri.EscapeDataString(request.TargetType)}";

            HttpResponseMessage response = await _httpClient.GetAsync(
                requestUri,
                cancellationToken);

            return await response.HandleResponseAsync<GetMediaAssetsByTargetResponse>(cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Network failure while getting media assets for target {TargetId} of type {TargetType} from File Service",
                request.TargetId,
                request.TargetType);

            return FileServiceClientErrors.Unavailable().ToErrors();
        }
    }
    
    public async Task<Result<AssetExistsResponse, Errors>> AssetExistsAsync(
        AssetExistsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"/files/{request.FileId}/exists",
                cancellationToken);

            return await response.HandleResponseAsync<AssetExistsResponse>(cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Network failure while checking media assets with id {id} exists from File Service",
                request.FileId);

            return FileServiceClientErrors.Unavailable().ToErrors();
        }
    }
}
