namespace DirectoryService.Shared;

public static class PositionErrors
{
    public static Error ToLongDescription() =>
        Error.Validation("too.long.description", "Описание слишком длинное.");
}