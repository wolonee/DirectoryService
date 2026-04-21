using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
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
        var departmentWithLocationsResult = await _departmentsRepository.GetByIdAsync(command.departmentId, cancellationToken);
        if (departmentWithLocationsResult.IsFailure)
            return departmentWithLocationsResult.Error.ToErrors();
        
        if (departmentWithLocationsResult.Value == null)
            return GeneralErrors.NotFound(null, "department").ToErrors();
        
        if (!departmentWithLocationsResult.Value.IsActive)
            return DepartmentErrors.IsNotActive().ToErrors();

        await _locationsRepository.DeleteLocationsByDepartmentId(command.departmentId, cancellationToken);
        
        var activeLocationsIdsResult = await _locationsRepository.GetActiveLocationsIdsAsync(command.request.LocationsIds, cancellationToken);
        if (activeLocationsIdsResult.IsFailure)
            return activeLocationsIdsResult.Error.ToErrors();
        
        if (activeLocationsIdsResult.Value.Count != command.request.LocationsIds.Length)
            return DepartmentErrors.NotAllLocationsActiveOrExists().ToErrors();

        // Update Locations

        // Save in database

        // Logging about success result
    }
}