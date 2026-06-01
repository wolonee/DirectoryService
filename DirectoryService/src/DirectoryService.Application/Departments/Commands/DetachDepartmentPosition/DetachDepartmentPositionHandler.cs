using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.DetachDepartmentPosition;

public class DetachDepartmentPositionHandler : ICommandHandler<DetachDepartmentPositionCommand>
{
    private readonly IValidator<DetachDepartmentPositionCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DetachDepartmentPositionHandler> _logger;

    public DetachDepartmentPositionHandler(
        IValidator<DetachDepartmentPositionCommand> validator,
        IDepartmentsRepository departmentsRepository,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        ILogger<DetachDepartmentPositionHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        DetachDepartmentPositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Detach Department Position Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        var positionResult = await _positionsRepository.GetByIdAsync(command.PositionId, cancellationToken);
        if (positionResult.IsFailure)
            return positionResult.Error.ToErrors();

        var deleteResult = await _departmentsRepository.DeleteDepartmentPositionAsync(
            command.DepartmentId,
            command.PositionId,
            cancellationToken);

        if (deleteResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }

        _logger.LogInformation(
            "Detached position {PositionId} from department {DepartmentId}",
            command.PositionId,
            command.DepartmentId);

        return UnitResult.Success<Errors>();
    }
}
