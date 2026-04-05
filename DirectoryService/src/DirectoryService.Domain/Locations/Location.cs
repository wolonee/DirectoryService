using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Domain.Locations;

public class Location
{
    // EF CORE
    private Location()
    {
    }
    
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

    public LocationTimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(LocationAddress addressResult, LocationName name, LocationTimeZone timezone, bool isActive)
    {
        return new Location(addressResult, name, timezone, isActive);
    }
}