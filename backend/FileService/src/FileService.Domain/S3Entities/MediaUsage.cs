using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.S3Entities;

public enum MediaUsage
{
    LESSON_VIDEO,
    COMPANY_PRESENTATION,
    COURSE_COVER,
    LOCATION_PHOTO,
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
            "location_photo" => MediaUsage.LOCATION_PHOTO,
            _ => GeneralErrors.ValueIsInvalid(nameof(MediaUsage)),
        };
    }
}
