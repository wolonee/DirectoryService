using DirectoryService.Shared;

namespace DirectoryService.Application.Exceptions;

public class TooManyLocationsException : BadRequestException
{
    public TooManyLocationsException(Error[] errors)
        : base(errors)
    {
    }
}