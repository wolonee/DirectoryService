
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Application.Locations;



public class CreateLocationHandler
{
    public CreateLocationHandler()
    {
    }
    
    public static async Task<Result<Guid>> Handle(CreateLocationAddressDto dto, CancellationToken ct = default)
    {
        // валидация входных данных
        // бизнес валидация
        
        // создание сущности Location
        var locationAddress = LocationAddress.Create(dto.Street, dto.City, dto.Country);
        if (locationAddress.IsFailure)
        {
            return Result.Failure<Guid>(locationAddress.Error);
        }

        var locationName = LocationName.Create(dto.Name);
        if (locationName.IsFailure)
        {
            return Result.Failure<Guid>(locationName.Error);
        }
        
        var locationTimezone = LocationTimeZone.Create(dto.Timezone);
        if (locationTimezone.IsFailure)
        {
            return Result.Failure<Guid>(locationTimezone.Error);
        }

        var location = Location.Create(locationAddress.Value, locationName.Value, locationTimezone.Value, dto.IsActive);
        
        // Сохранение сущности Location в базе данных
        
        // Логирование об успешном сохранении
    }
}