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

    public static Error DatabaseError() =>
        Error.Failure("directory.service.database.error", "Database exception with service - DirectoryService.");
    
    public static Error OperationCancelled() =>
        Error.Failure("directory.service.operation.cancelled", "Operation was cancelled.");
}