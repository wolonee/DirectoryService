using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public sealed record ContentType
{
    public string Value { get; }
    
    public MediaType MediaType { get; }
    
    private ContentType(string value, MediaType mediaType)
    {
        Value = value;
        MediaType = mediaType;
    }

    public static Result<ContentType, Error> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return GeneralErrors.ValueIsInvalid(nameof(contentType));

        MediaType category = contentType switch
        {
            var ct when ct.Contains("video", StringComparison.CurrentCultureIgnoreCase) => MediaType.VIDEO,
            var ct when ct.Contains("image", StringComparison.CurrentCultureIgnoreCase) => MediaType.IMAGE,
            var ct when ct.Contains("audio", StringComparison.CurrentCultureIgnoreCase) => MediaType.AUDIO,
            var ct when ct.Contains("document", StringComparison.CurrentCultureIgnoreCase) => MediaType.DOCUMENT,
            _ => MediaType.UNKNOWN
        };

        return new ContentType(contentType, category);
    }
}