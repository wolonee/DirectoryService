using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities.Assets;
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
    
    public Result<Guid, Error> Add(MediaAsset asset)
    {
        _dbContext.MediaAssets.Add(asset);
        
        return asset.Id;
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
}