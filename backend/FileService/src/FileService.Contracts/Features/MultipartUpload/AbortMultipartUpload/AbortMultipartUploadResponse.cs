namespace FileService.Contracts.Features.MultipartUpload.AbortMultipartUpload;

public sealed record AbortMultipartUploadResponse
{
    public Guid FileId { get; init; }

    public string Status { get; init; } = string.Empty;
}
