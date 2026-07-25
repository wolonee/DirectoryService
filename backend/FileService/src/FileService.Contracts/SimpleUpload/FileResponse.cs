namespace FileService.Contracts;

/// <summary>
/// Публичное представление media asset-а.
/// </summary>
/// <param name="FileId">Идентификатор asset-а.</param>
/// <param name="Status">Текущее состояние asset-а.</param>
/// <param name="FileName">Имя файла, заявленное при регистрации.</param>
/// <param name="ContentType">Ожидаемый MIME type.</param>
/// <param name="Size">Ожидаемый размер файла.</param>
/// <param name="Storage">Фактические metadata объекта; доступны после completion.</param>
/// <param name="ContentUrl">Свежий presigned GET URL только для готового asset-а.</param>
public sealed record FileResponse(
    Guid FileId,
    string Status,
    string FileName,
    string ContentType,
    long Size,
    ObjectMetadataDto? Storage,
    string? ContentUrl);
