using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public sealed record MediaData
{
    public FileName FileName { get; private init; } = null!;
    
    public ContentType ContentType { get; private init; } = null!;
    
    public long Size { get; private init; }
    
    private MediaData()
    {
    }

    private MediaData(FileName fileName, ContentType contentType, long size)
    {
        FileName = fileName;
        ContentType = contentType;
        Size = size;
    }

    public static Result<MediaData, Error> Create(FileName fileName, ContentType contentType, long size)
    {
        if (size <= 0)
            return GeneralErrors.ValueIsInvalid("size");
        
        return new MediaData(fileName, contentType, size);
    }
}
