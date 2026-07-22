namespace FileService.Infrastructure.S3;

public record S3Options
{
    public string Endpoint { get; init; } = string.Empty;
    
    public string AccessKey { get; init; } = string.Empty;
    
    public string SecretKey { get; init; } = string.Empty;
    
    public bool WithSsl { get; init; }
    
    public int DownloadUrlExpirationHours { get; init; }
    
    public int UploadUrlExpirationHours { get; init; }
    
    public int MaxConcurrentRequests { get; init; }

    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];
    
}