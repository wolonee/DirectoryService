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
        LocationTimeZone timezone)
    {
        Id = Guid.NewGuid();
        LocationAddress = locationAddress;
        Name = name;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public LocationAddress LocationAddress { get; private set; }

    public LocationName Name { get; private set; }

    public LocationTimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(LocationAddress addressResult, LocationName name, LocationTimeZone timezone)
    {
        return new Location(addressResult, name, timezone);
    }

    public Result Activate()
    {
        if (IsActive)
        {
            return Result.Failure("Location is already active");
        }
        
        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure("Location is not active");
        }
        
        IsActive = false;
        return Result.Success();
    }   
}