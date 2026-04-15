using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations;

public class Location
{
    // EF CORE
    private Location()
    {
    }
    
    private Location(
        LocationAddress address,
        LocationName name,
        LocationTimeZone timezone)
    {
        Id = Guid.NewGuid();
        Address = address;
        Name = name;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public LocationAddress Address { get; private set; }

    public LocationName Name { get; private set; }

    public LocationTimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(LocationAddress addressResult, LocationName name, LocationTimeZone timezone)
    {
        return new Location(addressResult, name, timezone);
    }

    public Result<bool, Error> Activate()
    {
        if (IsActive)
        {
            return LocationErrors.IsAlreadyActive(Id);
        }
        
        IsActive = true;
        return IsActive;
    }

    public Result<bool, Error> Deactivate()
    {
        if (!IsActive)
        {
            return LocationErrors.IsAlreadyInactive(Id);
        }
        
        IsActive = false;
        return IsActive;
    }   
}