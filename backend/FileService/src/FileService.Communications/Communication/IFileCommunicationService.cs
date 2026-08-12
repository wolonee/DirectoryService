using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.Simple.AssetExists;
using FileService.Contracts.Features.Simple.GetMediaAsset;
using FileService.Contracts.Features.Simple.GetMediaAssets;
using FileService.Contracts.Features.Simple.GetMediaAssetsByTarget;

namespace FileService.Communications.Communication;

public interface IFileCommunicationService
{
    Task<Result<GetMediaAssetResponse, Errors>> GetMediaAsset(GetMediaAssetRequest request, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssetsByIds(GetMediaAssetsRequest request, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsByTargetResponse, Errors>> GetMediaAssetsByTarget(GetMediaAssetsByTargetRequest request, CancellationToken cancellationToken);

    Task<Result<AssetExistsResponse, Errors>> AssetExistsAsync(AssetExistsRequest request, CancellationToken cancellationToken);
}