namespace DirectoryService.Shared;

public static class DepartmentErrors
{
    public static Error HasDuplicatedLocations() =>
        Error.Validation("locations.are.duplicated", $"Department has duplicated locations.");
}