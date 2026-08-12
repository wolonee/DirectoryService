using FileService.Domain.S3Entities.Assets;

namespace FileService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<MediaAsset> MediaAssetsQuery { get; }
}