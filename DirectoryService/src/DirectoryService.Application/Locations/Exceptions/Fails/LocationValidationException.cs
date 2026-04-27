using DirectoryService.Shared;
using DirectoryService.Shared.Exceptions;

namespace DirectoryService.Application.Locations.Exceptions.Fails;

public class LocationValidationException : BadRequestException
{
    public LocationValidationException(Errors errors)
        : base(errors)
    {
    }
}