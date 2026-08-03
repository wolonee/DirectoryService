namespace FileService.Contracts;

/// <summary>
/// Результат подтверждения загрузки файла.
/// </summary>
/// <param name="FileId">Идентификатор подтверждённого asset-а.</param>
/// <param name="Status">Новое состояние asset-а, обычно <c>READY</c>.</param>
/// <param name="Storage">Фактические metadata объекта, прочитанные из storage.</param>
public sealed record CompleteUploadResponse(
    Guid FileId,
    string Status,
    ObjectMetadataDto Storage);
