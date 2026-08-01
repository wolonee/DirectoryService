using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Getting media asset {FileId} from File Service was cancelled",
                request.FileId);

            throw;
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogWarning(
                exception,
                "Timed out while getting media asset {FileId} from File Service",
                request.FileId);

            return FileServiceClientErrors.Timeout().ToErrors();
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Getting {FileCount} media assets from File Service was cancelled",
                fileCount);

            throw;
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogWarning(
                exception,
                "Timed out while getting {FileCount} media assets from File Service",
                fileCount);

            return FileServiceClientErrors.Timeout().ToErrors();
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Getting media assets for target {TargetId} from File Service was cancelled",
                request.TargetId);

            throw;
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogWarning(
                exception,
                "Timed out while getting media assets for target {TargetId} from File Service",
                request.TargetId);

            return FileServiceClientErrors.Timeout().ToErrors();
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
}
