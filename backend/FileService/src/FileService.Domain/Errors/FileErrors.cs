using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public static class FileErrors
{
    public static Error BucketNotFound()
    {
        string name = string.Empty;

        return Error.NotFound(
            "no.such.bucket",
            $"Бакет не найден");
    }

    public static Error UploadNotFound()
    {
        return Error.NotFound(
            "upload.not.found",
            $"Сессия загрузки не найдена");
    }

    public static Error ObjectNotFound()
    {
        return Error.NotFound(
            "object.not.found",
            $"Объект не найден");
    }

    public static Error Forbidden()
    {
        return Error.Failure(
            "access.denied",
            "Недостаточно прав для выполнения операции");
    }

    public static Error ValidationFailed(string? reason = null)
    {
        string message = "Запрос содержит некорректные данные";

        if (!string.IsNullOrWhiteSpace(reason))
            message += $": {reason}";

        return Error.Validation(
            "validation.failed",
            message);
    }

    public static Error InternalServerError()
    {
        return Error.Failure(
            "internal.server.error",
            "Внутренняя ошибка хранилища");
    }

    public static Error OperationCanceled()
    {
        return Error.Failure(
            "operation.canceled",
            "Операция была отменена");
    }

    public static Error NetworkIssue()
    {
        return Error.Failure(
            "network.issue",
            "Сетевая ошибка при взаимодействии с файловым хранилищем");
    }

    public static Error Unknown()
    {
        return Error.Failure(
            "unknown.error",
            "Произошла неизвестная ошибка");
    }

    public static Error ProcessFailed()
    {
        return Error.Failure(
            "process.failed",
            "Внешний процесс завершился с ошибкой");
    }

    public static Error InvalidFfprobeOutput(string reason)
    {
        return Error.Failure(
            "ffprobe.invalid.output",
            $"Некорректный вывод ffprobe: {reason}");
    }

    public static Error HlsProcessingFailed(string? reason = null)
    {
        string message = "Не удалось сгенерировать HLS";

        if (!string.IsNullOrWhiteSpace(reason))
            message += $": {reason}";

        return Error.Failure(
            "hls.processing.failed",
            message);
    }
}