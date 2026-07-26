namespace FileService.Contracts;

/// <summary>
/// Результат soft-delete файла.
/// </summary>
public sealed record DeleteFileResponse
{
    public Guid FileId { get; init; }

    public string Status { get; init; } = string.Empty;
}
