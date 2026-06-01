using DirectoryService.Shared.Errors;

namespace DirectoryService.Shared.EntitiesErrors;

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
        Error.Validation("department.is.not.active", $"Department is not active.");
    
    public static Error ParentIdEqualDepartmentId() =>
        Error.Validation("parent.id.equal.department.id", $"Parent id can't be equal to department id.");
    
    public static Error DepartmentChildrensContainsParent() =>
        Error.Failure("department.childrens.contains.parent", $"Department childrens contains parent department.");

    public static Error HasActiveChildren() =>
        Error.Conflict("department.has.active.children", "Department has active child departments.");

    public static Error IsAlreadyInactive() =>
        Error.Conflict("department.is.already.inactive", "Department is already inactive.");

    public static Error DepartmentPositionAlreadyExists() =>
        Error.Conflict("department.position.already.exists", "Position is already linked to this department.");

    public static Error DepartmentPositionNotFound() =>
        Error.NotFound("department.position.not.found", "Position is not linked to this department.");
}