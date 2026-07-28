using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class ChunkSizeCalculator : IChunkSizeCalculator
{
    private readonly S3Options _options;

    public ChunkSizeCalculator(IOptions<S3Options> options)
    {
        _options = options.Value;
    }
    
    public Result<(long ChunkSize, int TotalChunks), Error> CalculateChunkSize(long fileSize)
    {
        int maxChunks = _options.MaxChunks;
        long minimumChunkSizeBytes = _options.MinimumChunkSizeBytes;
        long recommendedChunkSizeBytes = _options.RecommendedChunkSizeBytes;
        
        if (fileSize <= 0
            || minimumChunkSizeBytes < S3Options.S3MinimumPartSizeBytes
            || recommendedChunkSizeBytes < minimumChunkSizeBytes
            || maxChunks is <= 0 or > S3Options.S3MaximumPartsCount)
            return GeneralErrors.ValueIsInvalid("chunks setting");
        
        if (fileSize <= recommendedChunkSizeBytes)
            return (fileSize, 1);
        
        int calculatedChunks = (int)Math.Ceiling((double)fileSize / recommendedChunkSizeBytes);

        int actualChunks = Math.Min(calculatedChunks, maxChunks);
        
        long chunkSize = Math.Max(
            (fileSize + actualChunks - 1) / actualChunks,
            minimumChunkSizeBytes);
        
        return (chunkSize, actualChunks);
    }
}
