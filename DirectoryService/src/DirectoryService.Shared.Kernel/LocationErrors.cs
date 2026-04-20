namespace DirectoryService.Shared;

public static class LocationErrors
{
    public static Error ToManyLocations() =>
         Error.Failure("locations.too.many", "Пользователь не может открыть больше 3 локаций.");
    
    public static Error NotFound(Guid id) =>
        Error.NotFound("location.not.found", $"Локация c id: {id} не найдена.");    
    
    public static Error IsAlreadyActive(Guid id) =>
        Error.Failure("location.not.found", $"Location {id} is already active.");
    
    public static Error IsAlreadyInactive(Guid id) =>
        Error.Failure("location.not.found", $"Location {id} is already inactive.");
    
    // DataBase Errors
    public static Error NameConflict(string name) =>
        Error.Conflict("location.name.conflict", $"Location with name: {name} is already exists.");
    
    public static Error NameAlreadyExists(string name) =>
        Error.Failure("name.already.exists", $"Name: {name} already exists.");
    
    public static Error AddressAlreadyExists(string address) =>
        Error.Failure("name.already.exists", $"Address: {address} already exists.");
}