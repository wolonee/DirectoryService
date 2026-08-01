namespace FileService.Contracts;

public sealed record GetMediaAssetsRequest(IEnumerable<Guid> FileIds);
