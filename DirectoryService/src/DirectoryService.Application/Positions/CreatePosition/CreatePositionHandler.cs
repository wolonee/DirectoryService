using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IValidator<CreatePositionCommand> validator,
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        ILogger<CreatePositionHandler> logger)
    {
        _validator = validator;
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.request;
        
        // validation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Create Department Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }
        
        // business validation
        var activeNamesResult = await _positionsRepository.GetActiveFullNames(request.PositionName.Direction, request.PositionName.Speciality, cancellationToken);
        if (activeNamesResult.IsFailure)
            return activeNamesResult.Error.ToErrors();

        var requestFullName = PositionName.GetFullName(request.PositionName.Speciality, request.PositionName.Direction);

        foreach (var dbName in activeNamesResult.Value)
        {
            if (dbName == requestFullName)
                return PositionErrors.ActiveNameAlreadyExists().ToErrors();
        }

        var activeDepartmentsResult = await _departmentsRepository.GetActiveDepartmentsAsync(request.DepartmentIds, cancellationToken);
        if (activeDepartmentsResult.IsFailure)
            return activeDepartmentsResult.Error.ToErrors();
        
        if (!activeDepartmentsResult.Value.Any())
            return GeneralErrors.NotFound(null, "departments").ToErrors();

        if (activeDepartmentsResult.Value.Count != request.DepartmentIds.Length)
            return PositionErrors.NotAllDepartmentsExists().ToErrors();
        
        var positionId = Guid.NewGuid();

        // create position
        var name = PositionName.Create(request.PositionName.Speciality, request.PositionName.Direction).Value;
        
        var description = PositionDescription.Create(request.Description).Value;
        
        var positionResult = Position.Create(positionId, name, description).Value;
        
        // add position to each department
        foreach (var department in activeDepartmentsResult.Value)
        {
            var departmentPositionResult = DepartmentPosition.Create(department.Id, positionId);
            if (departmentPositionResult.IsFailure)
                return departmentPositionResult.Error.ToErrors();
            
            positionResult.AddDepartmentPosition(departmentPositionResult.Value);
            
            _logger.LogInformation("Position with id: {Id} was added into Department", department.Id);
        }

        // save position in db
        var saveResult = await _positionsRepository.AddAsync(positionResult, cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();
        
        // logger about success save
        _logger.LogInformation("Created Position with id {positionId}", saveResult.Value);   
        
        return saveResult.Value;
    }
}