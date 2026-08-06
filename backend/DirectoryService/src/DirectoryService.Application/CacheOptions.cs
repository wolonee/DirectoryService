namespace DirectoryService.Application;

public class CacheOptions
{
    public bool UseRedis { get; set; }
    
    public string RedisEndpoint { get; init; } = string.Empty;
    
    public TimeSpan LocalCacheExpiration { get; init; }
    
    public TimeSpan Expiration { get; init; }
}