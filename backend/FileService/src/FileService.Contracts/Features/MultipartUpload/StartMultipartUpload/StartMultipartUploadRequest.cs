namespace FileService.Contracts.Features.MultipartUpload.StartMultipartUpload;

public sealed record StartMultipartUploadRequest
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long Size { get; init; }

    public string AssetType { get; init; } = string.Empty;

    public string Usage { get; init; } = string.Empty;

    public string TargetType { get; init; } = string.Empty;

    public Guid TargetId { get; init; }
}
