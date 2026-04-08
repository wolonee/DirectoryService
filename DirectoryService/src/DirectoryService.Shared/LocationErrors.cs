using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.Exceptions;

public static class LocationErrors
{
    public static Error ToManyLocations() =>
         Error.Failure("locations.too.many", "Пользователь не может открыть больше 3 локаций.");
    
    public static Error NotFound(Guid id) =>
        Error.NotFound("location.not.found", $"Локация c id: {id} не найдена.");
}