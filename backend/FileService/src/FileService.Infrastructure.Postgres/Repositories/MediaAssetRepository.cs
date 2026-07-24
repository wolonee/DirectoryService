using FileService.Core.Abstractions;
using FileService.Domain.Assets;
using FileService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly FileServiceDbContext _dbContext;
    private readonly ILogger<MediaAssetRepository> _logger;

    public MediaAssetRepository(
        FileServiceDbContext dbContext,
        ILogger<MediaAssetRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task AddAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.MediaAssets.AddAsync(asset, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding asset");
            throw;
        }
    }

    public async Task<MediaAsset?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            MediaAsset? asset = await _dbContext.MediaAssets.FirstOrDefaultAsync(x => x.Id == fileId, cancellationToken: cancellationToken);
            
            return asset;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting asset");
            throw;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving asset");
            throw;
        }
    }
}
