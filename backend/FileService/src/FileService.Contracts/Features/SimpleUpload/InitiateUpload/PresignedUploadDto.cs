namespace FileService.Contracts;

public record PresignedUploadDto
{
    public string Url { get; init; } = string.Empty;
    
    public string Method { get; init; } = "PUT";
    
    public DateTime ExpiresAt { get; init; }
    
    public IReadOnlyDictionary<string, string> RequiredHeaders { get; init; } =
        new Dictionary<string, string>();
}
