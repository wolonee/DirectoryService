using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Errors = DirectoryService.Shared.Errors.Errors;

namespace DirectoryService.Application.Locations.Commands.UpdateLocation;

public class UpdateLocationHandler : ICommandHandler<UpdateLocationCommand>
{
    private readonly IValidator<UpdateLocationCommand> _validator;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateLocationHandler> _logger;

    public UpdateLocationHandler(
        IValidator<UpdateLocationCommand> validator,
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        ILogger<UpdateLocationHandler> logger)
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Update Location Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var locationResult = await _locationsRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var location = locationResult.Value;

        var dto = command.Request;

        // бизнес валидация — имя/адрес не должны принадлежать ДРУГОЙ локации
        var existsNameResult = await _locationsRepository.NameExistsAsync(dto.Name, command.LocationId, cancellationToken);
        if (existsNameResult.IsFailure)
            return existsNameResult.Error.ToErrors();

        if (existsNameResult.Value)
            return LocationErrors.NameAlreadyExists(dto.Name).ToErrors();

        var existsAddressResult = await _locationsRepository.AddressExistsAsync(dto.Address, command.LocationId, cancellationToken);
        if (existsAddressResult.IsFailure)
            return existsAddressResult.Error.ToErrors();

        if (existsAddressResult.Value)
            return LocationErrors.AddressAlreadyExists(dto.Address.ToString()).ToErrors();

        var locationAddress = LocationAddress.Create(dto.Address.Street, dto.Address.City, dto.Address.Country).Value;

        var locationName = LocationName.Create(dto.Name).Value;

        var locationTimezone = LocationTimeZone.Create(dto.Timezone).Value;

        location.Update(locationAddress, locationName, locationTimezone);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }

        _logger.LogInformation("Updated location {LocationId}", command.LocationId);

        return UnitResult.Success<Errors>();
    }
}
