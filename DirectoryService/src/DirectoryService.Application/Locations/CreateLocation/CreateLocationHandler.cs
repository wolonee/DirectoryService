
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations;



public class CreateLocationHandler
{
    private readonly ILocationsRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;
    
    public CreateLocationHandler(ILocationsRepository repository, ILogger<CreateLocationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<Result<Guid>> Handle(CreateLocationAddressDto dto, CancellationToken cancellationToken = default)
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
        var locationId = await _repository.AddAsync(location.Value, cancellationToken);

        // Логирование об успешном сохранении
        _logger.LogInformation("Created Location with id {locationId}", locationId);
        
        return locationId;
    }
}