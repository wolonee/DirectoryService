using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.Assets;

namespace FileService.Core.Abstractions;

public interface IMediaAssetRepository
{
    Result<Guid, Error> Add(MediaAsset asset);
    
    Task<Result<MediaAsset, Error>> GetByIdAsync(Guid fileId, CancellationToken cancellationToken);
}