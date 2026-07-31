namespace FileService.Contracts;

public sealed record CompleteMultipartUploadResponse
{
    public Guid FileId { get; init; }
}
