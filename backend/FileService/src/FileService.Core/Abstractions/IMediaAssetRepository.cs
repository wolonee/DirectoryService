using FileService.Domain.Assets;

namespace FileService.Core.Abstractions;

public interface IMediaAssetRepository
{
    Task AddAsync(MediaAsset asset, CancellationToken cancellationToken);
    
    Task<MediaAsset?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken);
    
    Task SaveChangesAsync(CancellationToken cancellationToken);
}