using System.Text.Json;

namespace DirectoryService.Shared.Exceptions;

public class BadRequestException : Exception
{
    protected BadRequestException(Errors errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}