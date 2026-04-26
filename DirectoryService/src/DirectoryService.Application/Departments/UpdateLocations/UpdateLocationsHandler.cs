using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
{
    private readonly IValidator<UpdateLocationsCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateLocationsHandler> _logger;

    public UpdateLocationsHandler(
        IValidator<UpdateLocationsCommand> validator,
        ILogger<UpdateLocationsHandler> logger, 
        IDepartmentsRepository departmentsRepository, 
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
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
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;
        
        var departmentResult = await _departmentsRepository.GetByIdAsync(command.departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }
        
        var department = departmentResult.Value;
        
        if (department == null)
            return GeneralErrors.NotFound(null, "department").ToErrors();
        
        if (!department.IsActive)
            return DepartmentErrors.IsNotActive().ToErrors();
        
        var activeLocationsIdsResult = await _locationsRepository.GetActiveLocationsIdsAsync(command.request.LocationsIds, cancellationToken);
        if (activeLocationsIdsResult.IsFailure)
        {
            return activeLocationsIdsResult.Error.ToErrors();
        }
        
        var locationIds = activeLocationsIdsResult.Value;
        
        if (locationIds == null)
            return DepartmentErrors.LocationsInvalid().ToErrors();
        
        if (locationIds.Count != command.request.LocationsIds.Length)
            return DepartmentErrors.NotAllLocationsActiveOrExists().ToErrors();
        
        // Update Locations
        var newDepartmentLocations = locationIds
            .Select(locationId => DepartmentLocation.Create(department, locationId).Value)
            .ToList();
        
        var updateResult = department.UpdateLocations(newDepartmentLocations);
        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();
        
        var deleteResult = await _departmentsRepository.DeleteLocationsByDepartmentId(command.departmentId, cancellationToken);
        if (deleteResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteResult.Error.ToErrors();
        }
        
        // Save in database
        await _transactionManager.SaveChangesAsync(cancellationToken);
        
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }
        
        // Logging about success result
        _logger.LogInformation("Updated department locations.");

        return departmentResult.Value.Id;
    }
}