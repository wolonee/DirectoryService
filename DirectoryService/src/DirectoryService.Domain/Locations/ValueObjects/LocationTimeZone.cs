    using CSharpFunctionalExtensions;
    using DirectoryService.Shared;

    namespace DirectoryService.Domain.Locations.ValueObjects;

public record LocationTimeZone
{
    private LocationTimeZone(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<LocationTimeZone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired();
        }

        if (!value.Contains('/'))
        {
            return Error.Validation("timezone.is.invalid", "Time zone must be in IANA format");
        }
        
        bool isValid = TimeZoneInfo.TryFindSystemTimeZoneById(value, out var _);
        if (isValid == false)
        {
            return Error.Validation("timezone.not.found", "timezone not found");
        }

        return new LocationTimeZone(value);
    }
}