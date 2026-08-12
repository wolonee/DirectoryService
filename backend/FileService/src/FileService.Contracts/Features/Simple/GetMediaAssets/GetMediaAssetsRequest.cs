namespace FileService.Contracts.Features.Simple.GetMediaAssets;

public sealed record GetMediaAssetsRequest(IEnumerable<Guid> FileIds);
