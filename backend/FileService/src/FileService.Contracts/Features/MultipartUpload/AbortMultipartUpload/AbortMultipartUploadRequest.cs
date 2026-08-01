namespace FileService.Contracts;

public sealed record AbortMultipartUploadRequest
{
    public Guid FileId { get; init; }

    public string UploadId { get; init; } = string.Empty;
}
