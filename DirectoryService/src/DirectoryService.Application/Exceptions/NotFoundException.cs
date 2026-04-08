using System.Text.Json;
using DirectoryService.Shared;

namespace DirectoryService.Application.Exceptions;

public class NotFoundException : Exception
{
    protected NotFoundException(Errors errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}