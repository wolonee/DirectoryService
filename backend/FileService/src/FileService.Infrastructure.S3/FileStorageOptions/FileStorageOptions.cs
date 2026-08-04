namespace FileService.Infrastructure.S3;

public record S3Options
{
    public const long S3MinimumPartSizeBytes = 5L * 1024 * 1024;

    public const int S3MaximumPartsCount = 10_000;

    public string Endpoint { get; init; } = string.Empty;
    
    public string ExternalEndpoint { get; init; } = string.Empty;
    
    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public bool WithSsl { get; init; }

    public TimeSpan DownloadUrlExpiration { get; init; }

    public TimeSpan UploadUrlExpiration { get; init; }

    public int MaxConcurrentRequests { get; init; }

    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];

    public long MinimumChunkSizeBytes { get; init; } = S3MinimumPartSizeBytes;

    public long RecommendedChunkSizeBytes { get; init; } = 100L * 1024 * 1024;

    public int MaxChunks { get; init; } = S3MaximumPartsCount;
}
