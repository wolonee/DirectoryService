namespace FileService.Contracts;

/// <summary>
/// Результат отмены незавершённой загрузки.
/// </summary>
public sealed record CancelUploadResponse
{
    public Guid FileId { get; init; }

    public string Status { get; init; } = string.Empty;
}
