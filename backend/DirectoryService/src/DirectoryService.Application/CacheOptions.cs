namespace DirectoryService.Application;

public class CacheOptions
{
    public string RedisEndpoint { get; init; } = string.Empty;
    
    public TimeSpan LocalCacheExpiration { get; init; }
    
    public TimeSpan Expiration { get; init; }
}