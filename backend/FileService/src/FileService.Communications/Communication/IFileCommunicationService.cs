using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.Contracts.HttpCommunication;

public interface IFileCommunicationService
{
    Task<Result<GetMediaAssetResponse, Errors>> GetMediaAsset(GetMediaAssetRequest request, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssetsByIds(GetMediaAssetsRequest request, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsByTargetResponse, Errors>> GetMediaAssetsByTarget(GetMediaAssetsByTargetRequest request, CancellationToken cancellationToken);
}
