using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentHandler : ICommandHandler<DeleteDepartmentCommand>
{
    private readonly IValidator<DeleteDepartmentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public DeleteDepartmentHandler(
        IValidator<DeleteDepartmentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        DeleteDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Delete Department Failed: {Error}", validationResult.ToValidationErrors());
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

        var hasChildrenResult = await _departmentsRepository.HasActiveChildDepartmentsAsync(
            command.DepartmentId,
            cancellationToken);

        if (hasChildrenResult.IsFailure)
        {
            transactionScope.Rollback();
            return hasChildrenResult.Error.ToErrors();
        }

        if (hasChildrenResult.Value)
            return DepartmentErrors.HasActiveChildren().ToErrors();

        var deletePositionsResult = await _departmentsRepository.DeleteDepartmentPositionsByDepartmentIdAsync(
            command.DepartmentId,
            cancellationToken);

        if (deletePositionsResult.IsFailure)
        {
            transactionScope.Rollback();
            return deletePositionsResult.Error.ToErrors();
        }

        var deleteLocationsResult = await _departmentsRepository.DeleteLocationsByDepartmentId(
            command.DepartmentId,
            cancellationToken);

        if (deleteLocationsResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteLocationsResult.Error.ToErrors();
        }

        var deactivateResult = department.Deactivate();
        if (deactivateResult.IsFailure)
            return deactivateResult.Error.ToErrors();

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

        _logger.LogInformation("Deleted department {DepartmentId}", command.DepartmentId);

        return UnitResult.Success<Errors>();
    }
}
