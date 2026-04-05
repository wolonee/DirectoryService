using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations.ValueObjects;

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

        if (value.Length < LengthConstants.LENGTH3 || value.Length > LengthConstants.LENGTH120)
        {
            return Result.Failure<LocationName>("Location name must be between 3 and 120 characters");
        }

        return new LocationName(value);
    }
}