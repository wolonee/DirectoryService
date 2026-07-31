using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record GetMediaAssetsByTargetQuery(GetMediaAssetsByTargetRequest Request) : IQuery;
