namespace FileService.Core.Options.CacheOptions;

public record CacheOptions
{
    /// <summary>Redis connection string for the distributed (L2) cache, e.g. "localhost:6379".</summary>
    public string RedisEndpoint { get; init; } = string.Empty;

    /// <summary>How long a signed URL lives in the cache. MUST be strictly less than FileStorageOptions.DownloadUrlExpiration.</summary>
    public TimeSpan PresignedUrlTtl { get; init; }

    /// <summary>How long a signed URL lives in the in-process (L1) cache. Should not exceed PresignedUrlTtl.</summary>
    public TimeSpan LocalCacheTtl { get; init; }
}
