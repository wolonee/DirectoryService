namespace DirectoryService.Shared;

public static class DepartmentErrors
{
    public static Error HasDuplicatedLocations() =>
        Error.Validation("locations.are.duplicated", $"Department has duplicated locations.");
    
    public static Error NotAllLocationsExists() =>
        Error.Validation("not.all.locations.exists", $"There are locations that don't exists.");
    
    public static Error NotAllLocationsActive() =>
        Error.Validation("not.all.locations.active", $"There are locations that don't active.");
    
    public static Error NotAllLocationsActiveOrExists() =>
        Error.Validation("not.all.locations.active.or.exists", $"There are locations that don't active or exists.");
    
    public static Error LocationsInvalid() =>
        Error.Validation("locations.invalid", $"All locations are invalid.");
    
    public static Error NameConflict(string name) =>
        Error.Conflict("department.name.conflict", $"Department with name: {name} already exists.");
    
    public static Error IsNotActive() =>
        Error.Failure("department.is.not.active", $"Department is not active.");
}