using System.Text.Json;

namespace DirectoryService.Shared.Exceptions;

public class NotFoundException : Exception
{
    protected NotFoundException(Errors errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}