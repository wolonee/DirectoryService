using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;


    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
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
            validationResult.ToValidationErrors();
        }
        
        // business validation
        // if (request.parentId == Guid.Empty)
        // {
        //     var depth = 0;
        //     
        // }
        // else
        // {
        //
        // }

        var resultLocationExists = await _locationsRepository.LocationsExistsAsync(request.locationIds, cancellationToken);
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
        var departmentLocationsList = request.locationIds
            .Select(locationId => DepartmentLocation.Create(departmentId, locationId).Value)
            .ToList();
        
        if (request.parentId == Guid.Empty)
        {
            var resultDepartment = Department.CreateParent(departmentName, departmentIdentifier, departmentLocationsList);
            if (resultDepartment.IsFailure)
            {
                return resultDepartment.Error.ToErrors();
            }
        }
        else
        {
            var resultParentDepartment = await _departmentsRepository.GetByIdAsync(request.parentId, cancellationToken);
            if (resultParentDepartment.IsFailure)
            {
                return resultParentDepartment.Error.ToErrors();
            }
            
            var resultDepartment = Department.CreateChild(departmentName, departmentIdentifier, resultParentDepartment.Value, departmentLocationsList);
            if (resultDepartment.IsFailure)
            {
                return resultDepartment.Error.ToErrors();
            }
        }

        // Сохранение в бд

        // логирование об успешном сохранении
    }
}