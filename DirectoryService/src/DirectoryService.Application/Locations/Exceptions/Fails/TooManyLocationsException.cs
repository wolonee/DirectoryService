using DirectoryService.Application.Exceptions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.Exceptions.Fails;

public class TooManyLocationsException : BadRequestException
{
    public TooManyLocationsException()
        : base([LocationErrors.ToManyLocations()])
    {
    }
}