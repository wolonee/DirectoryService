using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.Assets;

namespace FileService.Core.Abstractions;

public interface IMediaAssetRepository
{
    Task<Result<Guid, Error>> AddAsync(MediaAsset asset, CancellationToken cancellationToken);
    
    Task<Result<MediaAsset, Error>> GetByIdAsync(Guid fileId, CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}