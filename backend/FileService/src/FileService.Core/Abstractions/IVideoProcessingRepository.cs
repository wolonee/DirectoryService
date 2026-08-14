using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.Core.Abstractions;

public interface IVideoProcessingRepository
{
    Task<Result<VideoProcess, Error>> GetBy(
        Expression<Func<VideoProcess, bool>> predicate,
        CancellationToken cancellationToken = default);

    void Add(VideoProcess videoProcessing);
}