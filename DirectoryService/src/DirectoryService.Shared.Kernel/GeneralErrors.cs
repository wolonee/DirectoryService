namespace DirectoryService.Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "object";
        return Error.Failure("value.is.invalid", $"{label} is invalid");
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name ?? "object";
        return Error.Validation("value.is.required", $"{label} is required");
    }

    public static Error ValueHasBoundedLength(int minNameLength, int maxNameLength, string? name = null)
    {
        string label = name ?? "object";
        return Error.Validation("value.has.invalid.length", $"{label} must be between {minNameLength} and {maxNameLength} characters");
    }
    
    public static Error ValueContainsInvalidCharacters(string? name = null)
    {
        string label = name ?? "object";
        return Error.Validation("value.contains.invalid.characters", $"{label} only english letters (A-Z, a-z) are allowed, no spaces or special characters");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" with Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "object"} not found{forId}");
    }

    public static Error OperationCancelled() =>
        Error.Failure("directory.service.operation.cancelled", $"Operation with was cancelled.");
    
    public static Error DatabaseError() =>
        Error.Failure("directory.service.database.error", "Database exception with service - DirectoryService.");
}