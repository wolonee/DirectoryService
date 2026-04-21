using DirectoryService.Shared;
using DirectoryService.Shared.Exceptions;

namespace DirectoryService.Application.Locations.Exceptions.Fails;

public class TooManyLocationsException : BadRequestException
{
    public TooManyLocationsException()
        : base(LocationErrors.TooManyLocations())
    {
    }
}