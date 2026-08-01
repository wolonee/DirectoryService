using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.Infrastructure.S3;

public interface IChunkSizeCalculator
{
    Result<(long ChunkSize, int TotalChunks), Error> CalculateChunkSize(long fileSize);
}