namespace FileService.Contracts.Features.MultipartUpload.CompleteMultipartUpload;

public sealed record CompleteMultipartUploadResponse
{
    public Guid FileId { get; init; }
}
