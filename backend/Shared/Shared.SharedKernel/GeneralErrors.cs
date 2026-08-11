using DirectoryService.Shared.Errors;

namespace DirectoryService.Shared.EntitiesErrors;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "object";
        return Error.Failure("value.is.invalid", $"{label} is invalid");
    }
    
    public static Error Failure(string? message = null)
        => Error.Failure("failure", message ?? "unexpected failure");

    public static Error Duplicate(string? name = null)
    {
        string label = name ?? "object";
        return Error.Validation("value.has.duplicate", $"{label} has duplicates");
    }

    public static Error AlreadyExists(string? name = null)
    {
        string label = name ?? "object";
        return Error.Conflict("value.already.exists", $"{label} already exists");
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
    
    public static Error MaximumLength(int maxLength, string? name = null)
    {
        string label = name ?? "object";
        return Error.Validation("value.has.invalid.length", $"{label} must be less than {maxLength} characters");
    }
    
    public static Error MinimumLength(int minLength, string? name = null)
    {
        string label = name ?? "object";
        return Error.Validation("value.has.invalid.length", $"{label} must be more than {minLength} characters");
    }
    
    public static Error ValueContainsInvalidCharacters(string? message = null)
        => Error.Validation("value.contains.invalid.characters", message ?? "value contains invalid characters");

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" with Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "object"} not found{forId}");
    }

    public static Error OperationCancelled() =>
        Error.Failure("directory.service.operation.cancelled", "Operation was cancelled.");
    
    public static Error DatabaseError() =>
        Error.Failure("directory.service.database.error", "Database exception with service - DirectoryService.");
}