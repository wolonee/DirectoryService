namespace FileService.Contracts;

public sealed record StartMultipartUploadResponse
{
    public Guid FileId { get; init; }

    public string UploadId { get; init; } = string.Empty;

    public long ChunkSize { get; init; }

    public int TotalChunks { get; init; }

    public IReadOnlyList<MultipartPartUploadDto> Parts { get; init; } = [];
}
