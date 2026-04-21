namespace DirectoryService.Shared;

public static class PositionErrors
{
    public static Error ToLongDescription() =>
        Error.Validation("description.too.long", "Description is too long.");
    
    public static Error HasDuplicatedDepartments() =>
        Error.Validation("departments.are.duplicated", "Position has duplicated departments.");
    
    public static Error NotAllDepartmentsExists() =>
        Error.Validation("not.all.departments.exists", "There are departments that don't exist in database.");
    
    public static Error NotFoundNames() =>
        Error.NotFound("names.not.found", "No names found.");
    
    public static Error ActiveNameAlreadyExists() =>
        Error.Conflict("active.name.already.exists", "Active name already exists.");
    
    public static Error NameConflict(string name) =>
        Error.Conflict("position.name.conflict", $"Position with name: {name} is already exists.");
}