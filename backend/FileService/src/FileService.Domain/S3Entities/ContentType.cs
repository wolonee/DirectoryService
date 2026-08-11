using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.S3Entities;

public sealed record ContentType
{
    private static readonly Regex MimeTypeRegex = new(
        "^[a-zA-Z0-9!#$&^_.+-]+/[a-zA-Z0-9!#$&^_.+-]+$",
        RegexOptions.CultureInvariant);

    public string Value { get; private init; } = null!;
    
    public MediaType Category { get; private init; }

    private ContentType()
    {
    }

    private ContentType(string value, MediaType category)
    {
        Value = value;
        Category = category;
    }

    public static Result<ContentType, Error> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType) || !MimeTypeRegex.IsMatch(contentType))
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
