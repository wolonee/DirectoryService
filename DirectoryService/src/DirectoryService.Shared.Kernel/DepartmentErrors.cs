namespace DirectoryService.Shared;

public static class DepartmentErrors
{
    public static Error HasDuplicatedLocations() =>
        Error.Validation("locations.are.duplicated", $"Department has duplicated locations.");
    
    public static Error NotAllLocationsExists() =>
        Error.Validation("not.all.locations.exists", $"There are locations that don't exists in database.");
    
    public static Error NameConflict(string name) =>
        Error.Conflict("department.name.conflict", $"Department with name: {name} already exists.");
}