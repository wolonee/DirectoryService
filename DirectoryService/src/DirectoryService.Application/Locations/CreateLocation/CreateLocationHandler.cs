
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations;



public class CreateLocationHandler
{
    private readonly ILocationsRepository _repository;
    private readonly IValidator<CreateLocationDto> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;
    
    public CreateLocationHandler(ILocationsRepository repository, ILogger<CreateLocationHandler> logger, IValidator<CreateLocationDto> validator)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid>> Handle(CreateLocationDto dto, CancellationToken cancellationToken = default)
    {
        // валидация входных данных
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        if (!result.IsValid)
        {
            return Result.Failure<Guid>("Location not valid");
        }
        
        // бизнес валидация
        // например локаций не должно быть больше чем 10 и тд.
        
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