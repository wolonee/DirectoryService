using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
{
    private readonly IValidator<UpdateLocationsCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<UpdateLocationsHandler> _logger;

    public UpdateLocationsHandler(IValidator<UpdateLocationsCommand> validator, ILogger<UpdateLocationsHandler> logger, IDepartmentsRepository departmentsRepository, ILocationsRepository locationsRepository)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken = default)
    {
        // FluentValidation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        // Business validation
        var departmentResult = await _departmentsRepository.GetByIdAsync(command.departmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();
        
        if (departmentResult.Value == null)
            return GeneralErrors.NotFound(null, "department").ToErrors();
        
        if (!departmentResult.Value.IsActive)
            return DepartmentErrors.IsNotActive().ToErrors();

        await _locationsRepository.DeleteLocationsByDepartmentId(command.departmentId, cancellationToken);
        
        var activeLocationsIdsResult = await _locationsRepository.GetActiveLocationsIdsAsync(command.request.LocationsIds, cancellationToken);
        if (activeLocationsIdsResult.IsFailure)
            return activeLocationsIdsResult.Error.ToErrors();
        
        if (activeLocationsIdsResult.Value.Count != command.request.LocationsIds.Length)
            return DepartmentErrors.NotAllLocationsActiveOrExists().ToErrors();

        // Update Locations
        var newDepartmentLocations = activeLocationsIdsResult.Value
            .Select(locationId => DepartmentLocation.Create(departmentResult.Value.Id, locationId).Value)
            .ToList();
        
        departmentResult.Value.UpdateLocations(newDepartmentLocations);

        // Save in database
        await _departmentsRepository.SaveAsync();
        
        // Logging about success result
        _logger.LogInformation("Updated department locations.");

        return departmentResult.Value.Id;
    }
}