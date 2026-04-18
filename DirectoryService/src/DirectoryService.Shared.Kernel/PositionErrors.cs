namespace DirectoryService.Shared;

public static class PositionErrors
{
    public static Error ToLongDescription() =>
        Error.Validation("too.long.description", "Описание слишком длинное.");
    
    public static Error HasDuplicatedDepartments() =>
        Error.Validation("departments.are.duplicated", $"Position has duplicated departments.");
    
    public static Error NotAllDepartmentsExists() =>
        Error.Validation("not.all.departments.exists", $"There are departments that don't exists in database.");
}