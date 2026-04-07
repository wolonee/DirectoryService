using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.Exceptions;

public partial class Errors
{
    public static class Locations
    {
        public static Error ToManyLocations()
        {
            return Error.Failure("locations.too.many", "Пользователь не может открыть больше 3 локаций.");
        }
    }
}