using System.Net;
using DirectoryService.Shared.Errors;

namespace FileService.Communications.Communication.HttpCommunication;

public static class FileServiceClientErrors
{
    public static Error Unavailable() => Error.Failure(
        "file.service.unavailable",
        "File Service is temporarily unavailable.");

    public static Error Timeout() => Error.Failure(
        "file.service.timeout",
        "File Service did not respond within the configured timeout.");

    public static Error ServerDomainError(HttpStatusCode statusCode) => Error.Failure(
        "file.service.server.domain.error",
        $"File Service rejected the operation with HTTP status {(int)statusCode}.");

    public static Error ValidationError() => Error.Validation(
        "file.service.validation.error",
        "File Service rejected invalid request data.");

    public static Error NotFound() => Error.NotFound(
        "file.service.not.found",
        "Requested File Service resource was not found.");
}
