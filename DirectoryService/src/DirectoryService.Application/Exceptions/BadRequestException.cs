
using System.Text.Json;
using DirectoryService.Shared;

namespace DirectoryService.Application.Exceptions;

public class BadRequestException : Exception
{
    protected BadRequestException(Errors errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}