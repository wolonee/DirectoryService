using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
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
    
    public async Task<Result<Guid, Error>> AddAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.MediaAssets.AddAsync(asset, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return asset.Id;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding asset");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<MediaAsset, Error>> GetByIdAsync(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            MediaAsset? asset = await _dbContext.MediaAssets.FirstOrDefaultAsync(x => x.Id == fileId, cancellationToken: cancellationToken);
            if (asset == null)
            {
                _logger.LogError($"Asset with id {fileId} not found");
                return GeneralErrors.NotFound(fileId, "Asset");
            }
            
            return asset;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting asset");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving asset");
            return GeneralErrors.DatabaseError();
        }
    }
}
