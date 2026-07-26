namespace FileService.Contracts;

/// <summary>
/// Параметры поиска активных файлов, прикреплённых к внешней сущности.
/// </summary>
public sealed record GetFilesByTargetRequest
{
    public string TargetType { get; init; } = string.Empty;

    public Guid TargetId { get; init; }
}
