using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Exceptions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Errors = DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;
    
    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationCommand> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken = default)
    {
        var dto = command.Request;
        
        // валидация входных данных
        var resultValidation = await _validator.ValidateAsync(command, cancellationToken);
        if (!resultValidation.IsValid)
        {
            _logger.LogError("Validation Create Location Failed: {Error}", resultValidation.ToValidationErrors());
            return resultValidation.ToValidationErrors();
        }
        
        // бизнес валидация
        var existsNameResult = await _locationsRepository.NameExistsAsync(dto.Name, cancellationToken);
        if (existsNameResult.IsFailure)
            return existsNameResult.Error.ToErrors();

        if (!existsNameResult.Value)
            LocationErrors.NameAlreadyExists(dto.Name);
        
        var existsAddressResult = await _locationsRepository.AddressExistsAsync(dto.Address, cancellationToken);
        if (existsAddressResult.IsFailure)
            return existsAddressResult.Error.ToErrors();
        
        if (!existsAddressResult.Value)
            LocationErrors.AddressAlreadyExists(dto.Address.ToString());
        
        // создание сущности Location
        var addressDto = dto.Address;
        var locationAddress = LocationAddress.Create(addressDto.Street, addressDto.City, addressDto.Country).Value;

        var locationName = LocationName.Create(dto.Name).Value;
        
        var locationTimezone = LocationTimeZone.Create(dto.Timezone).Value;

        var location = Location.Create(locationAddress, locationName, locationTimezone);
        
        // Сохранение сущности Location в базе данных
        var saveResult = await _locationsRepository.AddAsync(location.Value, cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        // Логирование об успешном сохранении
        _logger.LogInformation("Created Location with id {locationId}", saveResult);
        
        return saveResult.Value;
    }
}
