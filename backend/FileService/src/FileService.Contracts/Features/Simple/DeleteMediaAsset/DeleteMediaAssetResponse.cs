namespace FileService.Contracts.Features.Simple.DeleteMediaAsset;

/// <summary>
/// Результат soft-delete файла.
/// </summary>
public sealed record DeleteMediaAssetResponse
{
    public Guid FileId { get; init; }

    public string Status { get; init; } = string.Empty;
}
