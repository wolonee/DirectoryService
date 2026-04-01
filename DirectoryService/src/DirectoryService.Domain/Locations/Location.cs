using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Domain.Locations;

public class Location
{
    private Location(
        LocationAddress locationAddress,
        LocationName name,
        LocationTimeZone timezone,
        bool isActive)
    {
        Id = Guid.NewGuid();
        LocationAddress = locationAddress;
        Name = name;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public LocationAddress LocationAddress { get; private set; }

    public LocationName? Name { get; private set; }

    public LocationTimeZone? Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(string street, string city, string country, string name, string timezone, bool isActive)
    {
        var addressResult = LocationAddress.Create(street, city, country);
        if (addressResult.IsFailure)
        {
            return Result.Failure<Location>(addressResult.Error);
        }

        var nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<Location>(nameResult.Error);
        }
        
        var timezoneResult = LocationTimeZone.Create(timezone);
        if (timezoneResult.IsFailure)
        {
            return Result.Failure<Location>(timezoneResult.Error);
        }
        
        var validAddress = addressResult.Value;
        var validName = nameResult.Value;
        var validTimezone = timezoneResult.Value;
        
        return new Location(validAddress, validName, validTimezone, isActive);
    }
}