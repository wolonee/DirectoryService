using DirectoryService.Application.Exceptions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.Exceptions.Fails;

public class LocationValidationException : BadRequestException
{
    public LocationValidationException(Errors errors)
        : base(errors)
    {
    }
}