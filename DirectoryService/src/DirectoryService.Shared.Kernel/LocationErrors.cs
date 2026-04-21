namespace DirectoryService.Shared;

public static class LocationErrors
{
    public static Error TooManyLocations() =>
        Error.Conflict("locations.too.many", "User cannot open more than 3 locations.");
    
    public static Error NotFound(Guid id) =>
        Error.NotFound("location.not.found", $"Location with id: {id} not found.");
    
    public static Error IsAlreadyActive(Guid id) =>
        Error.Conflict("location.is.already.active", $"Location {id} is already active.");
    
    public static Error IsAlreadyInactive(Guid id) =>
        Error.Conflict("location.is.already.inactive", $"Location {id} is already inactive.");
    
    // DataBase Errors
    public static Error NameConflict(string name) =>
        Error.Conflict("location.name.conflict", $"Location with name: {name} already exists.");
    
    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("location.name.already.exists", $"Name: {name} already exists.");
    
    public static Error AddressAlreadyExists(string address) =>
        Error.Conflict("location.address.already.exists", $"Address: {address} already exists.");
}