namespace FileService.Contracts.Features.Simple.GetMediaAssets;

public sealed record GetMediaAssetsResponse(IEnumerable<GetMediaAssetDto> MediaAssets);