using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.Assets;
using FileService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

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
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException)
        {
            _logger.LogError(ex, "Postgres error while adding media asset {MediaAssetId}", asset.Id);
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Adding media asset {MediaAssetId} was cancelled", asset.Id);
            return GeneralErrors.DatabaseError();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding media asset {MediaAssetId}", asset.Id);
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
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException)
        {
            _logger.LogError(ex, "Postgres error while saving media assets");
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Saving media assets was cancelled");
            return GeneralErrors.DatabaseError();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving media assets");
            return GeneralErrors.DatabaseError();
        }
    }
}
