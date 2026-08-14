using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.Assets;

namespace FileService.Core.Abstractions;

public interface IVideoAssetRepository
{
    Task<Result<VideoAsset, Error>> GetByIdAsync(Guid videoId, CancellationToken cancellationToken);
}