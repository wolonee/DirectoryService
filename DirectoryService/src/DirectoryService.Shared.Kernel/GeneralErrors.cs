namespace DirectoryService.Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Failure("value.is.invalid", $"{label} недействительно");
    }
    
    public static Error ValueIsRequired(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.required", $"{label} обязательно");
    }
    
    public static Error ValueHasBoundedLength(int minNameLength, int maxNameLength, string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.required", $"{label} name must be between {minNameLength} and {maxNameLength} characters");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" no Id '{id}';";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}");
    }
}