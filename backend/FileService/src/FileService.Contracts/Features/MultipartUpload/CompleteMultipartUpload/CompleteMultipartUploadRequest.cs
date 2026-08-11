namespace FileService.Contracts.Features.MultipartUpload.CompleteMultipartUpload;

public sealed record CompleteMultipartUploadRequest
{
    public Guid FileId { get; init; }

    public string UploadId { get; init; } = string.Empty;

    public IReadOnlyList<PartETagDto> Parts { get; init; } = [];
}
