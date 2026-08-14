using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities.MediaProcessing;
using FileService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.Repositories;

public class VideoProcessingRepository : IVideoProcessingRepository
{
    private readonly FileServiceDbContext _dbContext;

    public VideoProcessingRepository(FileServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<VideoProcess, Error>> GetBy(
        Expression<Func<VideoProcess, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        VideoProcess? videoProcessing = await _dbContext.VideoProcesses
            .Include(v => v.Steps)
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (videoProcessing is null)
            return GeneralErrors.NotFound();

        return videoProcessing;
    }

    public void Add(VideoProcess videoProcessing)
    {
        _dbContext.VideoProcesses.Add(videoProcessing);
    }
}