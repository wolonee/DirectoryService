namespace FileService.Contracts;

/// <summary>
/// Список активных файлов внешней сущности.
/// </summary>
public sealed record FilesByTargetResponse
{
    public string TargetType { get; init; } = string.Empty;

    public Guid TargetId { get; init; }

    public IReadOnlyList<GetMediaAssetResponse> Files { get; init; } = [];
}
