namespace FileService.Contracts.Features.Simple.GetMediaAssetsByTarget;

/// <summary>
/// Параметры поиска активных файлов, прикреплённых к внешней сущности.
/// </summary>
public sealed record GetMediaAssetsByTargetRequest
{
    public string TargetType { get; init; } = string.Empty;

    public Guid TargetId { get; init; }
}
