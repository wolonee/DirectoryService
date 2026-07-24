using DirectoryService.Shared.Errors;

namespace FileService.Domain;

/// <summary>
/// Доменные ошибки работы с media asset во время upload completion.
/// </summary>
public static class MediaAssetErrors
{
    public static Error NotFound(Guid fileId) =>
        Error.NotFound(
            "media-asset.not-found",
            $"Media asset '{fileId}' was not found.");

    public static Error WrongUploader() =>
        Error.Failure(
            "media-asset.wrong-uploader",
            "The current user is not the uploader of this media asset.");

    public static Error InvalidStatus(string status) =>
        Error.Conflict(
            "media-asset.invalid-status",
            $"Media asset cannot be completed from status '{status}'.");

    public static Error AlreadyCompleted() =>
        Error.Conflict(
            "media-asset.already-completed",
            "Media asset has already been completed.");

    public static Error SizeMismatch(long expected, long actual) =>
        Error.Validation(
            "media-asset.size-mismatch",
            $"Uploaded object size '{actual}' does not match expected size '{expected}'.");

    public static Error ContentTypeMismatch(string expected, string actual) =>
        Error.Validation(
            "media-asset.content-type-mismatch",
            $"Uploaded object content type '{actual}' does not match expected content type '{expected}'.");
}
