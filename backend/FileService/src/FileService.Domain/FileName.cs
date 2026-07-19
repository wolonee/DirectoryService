using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public sealed record FileName
{
    public string Name { get; }
    public string Extention { get; }

    private FileName(string name, string extention)
    {
        Name = name;
        Extention = extention;
    }

    public static Result<FileName, Error> Create(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return GeneralErrors.ValueIsInvalid(nameof(fileName));
        
        int lastDot = fileName.LastIndexOf('.');
        if (lastDot == -1 || lastDot == fileName.Length - 1)
            return GeneralErrors.ValueIsInvalid("File must have extention");
        
        string extention = fileName[(lastDot + 1)..].ToLowerInvariant();
        return new FileName(fileName, extention);
    }
}