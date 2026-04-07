using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Exceptions;
using DirectoryService.Application.Locations.Exceptions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;
    private readonly IValidator<CreateLocationRequest> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;
    
    public CreateLocationHandler(
        ILocationsRepository repository,
        ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationRequest> validator)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error[]>> Handle(CreateLocationCommand command, CancellationToken cancellationToken = default)
    {
        var dto = command.Request;
        
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

        var locationName = LocationName.Create(dto.Name);
        
        var locationTimezone = LocationTimeZone.Create(dto.Timezone);

        var location = Location.Create(locationAddress.Value, locationName.Value, locationTimezone.Value);
        
        // Сохранение сущности Location в базе данных
        var saveResult = await _repository.AddAsync(location.Value, cancellationToken);

        // Логирование об успешном сохранении
        _logger.LogInformation("Created Location with id {locationId}", saveResult);
        
        return saveResult;
    } 
}
