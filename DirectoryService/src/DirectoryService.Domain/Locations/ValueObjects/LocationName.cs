using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record LocationName
{
    private LocationName(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LocationName>("Location name cannot be empty");
        }

        return new LocationName(value);
    }
}