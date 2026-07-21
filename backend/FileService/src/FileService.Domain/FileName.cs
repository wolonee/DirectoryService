using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public sealed record FileName
{
    public const int MAX_LENGTH = 255;

    public string Name { get; private init; } = null!;
    public string Extension { get; private init; } = null!;

    private FileName()
    {
    }

    private FileName(string name, string extension)
    {
        Name = name;
        Extension = extension;
    }

    public static Result<FileName, Error> Create(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > MAX_LENGTH)
            return GeneralErrors.ValueIsInvalid(nameof(fileName));
        
        int lastDot = fileName.LastIndexOf('.');
        if (lastDot == -1 || lastDot == fileName.Length - 1)
            return GeneralErrors.ValueIsInvalid("File must have extension");
        
        string extension = fileName[(lastDot + 1)..].ToLowerInvariant();
        return new FileName(fileName, extension);
    }
}
