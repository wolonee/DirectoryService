using DirectoryService.Application.Locations.Exceptions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Exceptions;

public class TooManyLocationsException : BadRequestException
{
    public TooManyLocationsException()
        : base([Errors.Locations.ToManyLocations()])
    {
    }
}