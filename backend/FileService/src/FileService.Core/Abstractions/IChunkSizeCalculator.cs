using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.Core.Abstractions;

public interface IChunkSizeCalculator
{
    Result<(long ChunkSize, int TotalChunks), Error> CalculateChunkSize(long fileSize);
}
