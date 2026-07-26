namespace FileService.Contracts;

public sealed record GetMediaAssetsResponse(IEnumerable<GetMediaAssetDto> MediaAssets);