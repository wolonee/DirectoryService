using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.AttachDepartmentPosition;

public class AttachDepartmentPositionHandler : ICommandHandler<AttachDepartmentPositionCommand>
{
    private readonly IValidator<AttachDepartmentPositionCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<AttachDepartmentPositionHandler> _logger;

    public AttachDepartmentPositionHandler(
        IValidator<AttachDepartmentPositionCommand> validator,
        IDepartmentsRepository departmentsRepository,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        ILogger<AttachDepartmentPositionHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        AttachDepartmentPositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Attach Department Position Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        var department = departmentResult.Value;

        if (!department.IsActive)
            return DepartmentErrors.IsNotActive().ToErrors();
        
        if (department.IsDeleted)
            return DepartmentErrors.IsDeleted().ToErrors();

        var positionResult = await _positionsRepository.GetByIdAsync(command.PositionId, cancellationToken);
        if (positionResult.IsFailure)
            return positionResult.Error.ToErrors();

        var position = positionResult.Value;

        if (!position.IsActive)
            return PositionErrors.IsNotActive().ToErrors();

        var linkExistsResult = await _departmentsRepository.DepartmentPositionExistsAsync(command.DepartmentId, command.PositionId, cancellationToken);

        if (linkExistsResult.IsFailure)
        {
            transactionScope.Rollback();
            return linkExistsResult.Error.ToErrors();
        }

        if (linkExistsResult.Value)
            return DepartmentErrors.DepartmentPositionAlreadyExists().ToErrors();

        var departmentPositionResult = DepartmentPosition.Create(command.DepartmentId, command.PositionId);
        if (departmentPositionResult.IsFailure)
            return departmentPositionResult.Error.ToErrors();

        var addResult = await _departmentsRepository.AddDepartmentPositionAsync(
            departmentPositionResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
        {
            transactionScope.Rollback();
            return addResult.Error.ToErrors();
        }

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

        _logger.LogInformation(
            "Attached position {PositionId} to department {DepartmentId}",
            command.PositionId,
            command.DepartmentId);

        return UnitResult.Success<Errors>();
    }
}
