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
}