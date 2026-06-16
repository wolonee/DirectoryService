using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

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
    
    public bool IsDeleted { get; private set; }
    
    public DateTime? DeletedAt { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(LocationAddress addressResult, LocationName name, LocationTimeZone timezone)
    {
        return new Location(addressResult, name, timezone);
    }

    public void Activate(bool boolean)
    {
        IsActive = boolean;
        UpdatedAt = DateTime.UtcNow;
    }

    public UnitResult<Error> Deactivate()
    {
        if (!IsActive)
            return LocationErrors.IsAlreadyInactive();

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
    
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}