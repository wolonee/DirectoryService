using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public enum MediaUsage
{
    LESSON_VIDEO,
    COMPANY_PRESENTATION,
    COURSE_COVER,
}

public static class MediaUsageExtensions
{
    public static Result<MediaUsage, Error> ToMediaUsage(this string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "lesson_video" => MediaUsage.LESSON_VIDEO,
            "company_presentation" => MediaUsage.COMPANY_PRESENTATION,
            "course_cover" => MediaUsage.COURSE_COVER,
            _ => GeneralErrors.ValueIsInvalid(nameof(MediaUsage)),
        };
    }
}
