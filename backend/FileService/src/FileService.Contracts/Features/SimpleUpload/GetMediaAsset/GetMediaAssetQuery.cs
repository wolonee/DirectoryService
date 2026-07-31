using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record GetMediaAssetQuery(Guid FileId) : IQuery;
