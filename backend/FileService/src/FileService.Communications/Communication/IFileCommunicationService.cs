using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.Contracts.HttpCommunication;

public interface IFileCommunicationService
{
    Task<Result<GetMediaAssetResponse, Errors>> GetMediaAsset(GetMediaAssetQuery query, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssetsByIds(GetMediaAssetsQuery query, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsByTargetResponse, Errors>> GetMediaAssetsByTarget(GetMediaAssetsByTargetQuery query, CancellationToken cancellationToken);
}
