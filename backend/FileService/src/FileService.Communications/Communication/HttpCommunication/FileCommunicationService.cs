using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.Extensions.Logging;

namespace FileService.Contracts.HttpCommunication;

internal class FileCommunicationService : IFileCommunicationService
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
        GetMediaAssetQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"/files/{query.FileId}",
                cancellationToken);

            return await response
                .HandleResponseAsync<GetMediaAssetResponse>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Getting media asset {FileId} from File Service was cancelled",
                query.FileId);

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to get media asset {FileId} from File Service",
                query.FileId);

            return GeneralErrors
                .Failure("File Service communication failed while getting media asset")
                .ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssetsByIds(
        GetMediaAssetsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/files", query, cancellationToken);

            return await response
                .HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Getting {FileCount} media assets from File Service was cancelled",
                query.FileIds.Count());

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to get {FileCount} media assets from File Service",
                query.FileIds.Count());

            return GeneralErrors
                .Failure("File Service communication failed while getting media assets")
                .ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetsByTargetResponse, Errors>> GetMediaAssetsByTarget(
        GetMediaAssetsByTargetQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = query.Request;

            string requestUri = $"/files?TargetId={request.TargetId}";

            if (!string.IsNullOrWhiteSpace(request.TargetType))
            {
                requestUri += $"&TargetType={Uri.EscapeDataString(request.TargetType)}";
            }

            HttpResponseMessage response = await _httpClient.GetAsync(
                requestUri,
                cancellationToken);

            return await response
                .HandleResponseAsync<GetMediaAssetsByTargetResponse>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Getting media assets for target {TargetId} from File Service was cancelled",
                query.Request.TargetId);

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to get media assets for target {TargetId} of type {TargetType} from File Service",
                query.Request.TargetId,
                query.Request.TargetType);

            return GeneralErrors
                .Failure("File Service communication failed while getting media assets by target")
                .ToErrors();
        }
    }
}
