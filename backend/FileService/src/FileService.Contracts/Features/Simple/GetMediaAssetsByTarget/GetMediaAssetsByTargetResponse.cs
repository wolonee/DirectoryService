namespace FileService.Contracts.Features.Simple.GetMediaAssetsByTarget;

/// <summary>
/// Список активных файлов внешней сущности.
/// </summary>
public sealed record GetMediaAssetsByTargetResponse
{
    public string TargetType { get; init; } = string.Empty;

    public Guid TargetId { get; init; }

    public IReadOnlyList<GetMediaAssetByTargetDto> Files { get; init; } = [];
}