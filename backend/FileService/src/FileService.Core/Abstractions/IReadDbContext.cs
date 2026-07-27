using FileService.Domain.Assets;

namespace FileService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<MediaAsset> MediaAssetsQuery { get; }
}