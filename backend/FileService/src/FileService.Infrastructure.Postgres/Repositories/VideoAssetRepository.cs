using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities.Assets;
using FileService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.Postgres.Repositories;

public class VideoAssetRepository : IVideoAssetRepository
{
    private readonly FileServiceDbContext _dbContext;
    private readonly ILogger<MediaAssetRepository> _logger;

    public VideoAssetRepository(
        FileServiceDbContext dbContext,
        ILogger<MediaAssetRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<VideoAsset, Error>> GetByIdAsync(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            MediaAsset? asset = await _dbContext.MediaAssets.FirstOrDefaultAsync(
                x => x.Id == fileId, 
                cancellationToken: cancellationToken);
            if (asset == null)
            {
                _logger.LogError($"Asset with id {fileId} not found");
                return GeneralErrors.NotFound(fileId, "Asset");
            }

            if (asset is not VideoAsset videoAsset)
            {
                _logger.LogError($"Asset with id {fileId} is not a video asset");
                return GeneralErrors.NotFound(fileId, "VideoAsset");
            }

            return videoAsset;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting asset");
            return GeneralErrors.DatabaseError();
        }
    }
}