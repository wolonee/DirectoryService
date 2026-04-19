using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;


    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository, ILogger<CreateDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.request;
        
        // validation
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Create Department Failed: {Error}", validationResult.ToValidationErrors());
            validationResult.ToValidationErrors();
        }

        var resultLocationExists = await _locationsRepository.LocationsExistsAsync(request.LocationIds, cancellationToken);
        if (resultLocationExists.IsFailure)
        {
            return resultLocationExists.Error.ToErrors();
        }

        if (resultLocationExists.Value == false)
        {
            return DepartmentErrors.NotAllLocationsExists().ToErrors();
        }

        // Создание сущности Department
        var departmentName = DepartmentName.Create(request.Name).Value;
        
        var departmentIdentifier = DepartmentIdentifier.Create(request.Identifier).Value;
        
        Guid departmentId = Guid.NewGuid();
        
        var departmentLocationsList = request.LocationIds
            .Select(locationId => DepartmentLocation.Create(departmentId, locationId).Value)
            .ToList();
        
        if (request.ParentId == null)
        {
            var resultDepartment = Department.CreateParent(departmentId, departmentName, departmentIdentifier, departmentLocationsList);
            if (resultDepartment.IsFailure)
                return resultDepartment.Error.ToErrors();
            
            // Сохранение в бд
            var saveResult = await _departmentsRepository.AddAsync(resultDepartment.Value, cancellationToken);
            if (saveResult.IsFailure)
                return saveResult.Error.ToErrors();
            
            // логирование об успешном сохранении
            _logger.LogInformation("Created Parent Department with id {departmentId}", saveResult.Value);
            
            return saveResult.Value;
        }
        else
        {
            var resultParentDepartment = await _departmentsRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (resultParentDepartment.IsFailure)
                return resultParentDepartment.Error.ToErrors();
            
            var resultDepartment = Department.CreateChild(departmentId, departmentName, departmentIdentifier, resultParentDepartment.Value, departmentLocationsList);
            if (resultDepartment.IsFailure)
                return resultDepartment.Error.ToErrors();
            
            // Сохранение в бд
            var saveResult = await _departmentsRepository.AddAsync(resultDepartment.Value, cancellationToken);
            if (saveResult.IsFailure)
                return saveResult.Error.ToErrors();
            
            // логирование об успешном сохранении
            _logger.LogInformation("Created Child Department with id {departmentId}", saveResult.Value);
            
            return saveResult.Value;
        }
    }
}