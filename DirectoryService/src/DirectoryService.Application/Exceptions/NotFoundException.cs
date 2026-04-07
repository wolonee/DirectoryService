using System.Text.Json;
using DirectoryService.Shared;

namespace DirectoryService.Application.Exceptions;

public class NotFoundException : Exception
{
    protected NotFoundException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}