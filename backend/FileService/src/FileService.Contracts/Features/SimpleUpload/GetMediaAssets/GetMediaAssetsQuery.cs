using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record GetMediaAssetsQuery(IEnumerable<Guid> FileIds) : IQuery;
