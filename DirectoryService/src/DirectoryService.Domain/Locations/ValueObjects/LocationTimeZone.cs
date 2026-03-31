    using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record LocationTimeZone
{
    private LocationTimeZone(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<LocationTimeZone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LocationTimeZone>("Location name cannot be empty");
        }

        if (!value.Contains('/'))
        {
            return Result.Failure<LocationTimeZone>("Time zone must be in IANA format");
        }
        
        bool isValid = TimeZoneInfo.TryFindSystemTimeZoneById(value, out var _);
        if (isValid == false)
        {
            return Result.Failure<LocationTimeZone>("Time zone not found");
        }

        return new LocationTimeZone(value);
    }
}