using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.Commands.DeletePosition;

public class DeletePositionHandler : ICommandHandler<DeletePositionCommand>
{
    private readonly IValidator<DeletePositionCommand> _validator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeletePositionHandler> _logger;

    public DeletePositionHandler(
        IValidator<DeletePositionCommand> validator,
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<DeletePositionHandler> logger)
    {
        _validator = validator;
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        DeletePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Delete Position Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var positionResult = await _positionsRepository.GetByIdAsync(command.PositionId, cancellationToken);
        if (positionResult.IsFailure)
            return positionResult.Error.ToErrors();

        var position = positionResult.Value;

        var deleteLinksResult = await _departmentsRepository.DeleteDepartmentPositionsByPositionIdAsync(
            command.PositionId,
            cancellationToken);

        if (deleteLinksResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteLinksResult.Error.ToErrors();
        }

        var deactivateResult = position.Deactivate();
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

        _logger.LogInformation("Deleted position {PositionId}", command.PositionId);

        return UnitResult.Success<Errors>();
    }
}
