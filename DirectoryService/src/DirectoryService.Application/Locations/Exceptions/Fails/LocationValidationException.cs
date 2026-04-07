using DirectoryService.Shared;

namespace DirectoryService.Application.Exceptions;

public class LocationValidationException : BadRequestException
{
    public LocationValidationException(Error[] errors)
        : base(errors)
    {
    }
}