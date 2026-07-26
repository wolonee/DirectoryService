using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class ChunkSizeCalculator
{
    private readonly S3Options _options;

    public ChunkSizeCalculator(IOptions<S3Options> options)
    {
        _options = options.Value;
    }
    
    public Result<(long ChunkSize, int TotalChunks), Error> CalculateChunkSize(long fileSize)
    {
        int maxChunks = _options.MaxChucks;
        long recommendedChunkSizeBytes = _options.RecommendedChunkSizeBytes;
        
        if (recommendedChunkSizeBytes <= 0 || maxChunks <= 0)
            return GeneralErrors.ValueIsInvalid("chunks setting");
        
        if (fileSize <= recommendedChunkSizeBytes)
            return (fileSize, 1);
        
        int calculatedChunks = (int)Math.Ceiling((double)fileSize / recommendedChunkSizeBytes);

        int actualChunks = Math.Min(calculatedChunks, maxChunks);
        
        long chunkSize = fileSize / actualChunks;
        
        return (chunkSize, actualChunks);
    }
}