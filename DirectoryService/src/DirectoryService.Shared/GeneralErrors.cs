namespace DirectoryService.Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Failure("value.is.invalid", $"{label} недействительно");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" no Id '{id}';";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}");
    }
}