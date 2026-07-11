using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.Commands.RenamePosition;

public class RenamePositionHandler : ICommandHandler<Guid, RenamePositionCommand>
{
    private readonly IValidator<RenamePositionCommand> _validator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RenamePositionHandler> _logger;

    public RenamePositionHandler(
        IValidator<RenamePositionCommand> validator,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        ILogger<RenamePositionHandler> logger)
    {
        _validator = validator;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        RenamePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Rename Position Failed: {Error}", validationResult.ToValidationErrors());
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

        if (!position.IsActive)
            return PositionErrors.IsNotActive().ToErrors();
        
        if (position.IsDeleted)
            return PositionErrors.IsDeleted().ToErrors();

        var nameExistsResult = await _positionsRepository.ActiveFullNameExistsAsync(
            command.Request.PositionName.Direction,
            command.Request.PositionName.Speciality,
            command.PositionId,
            cancellationToken);

        if (nameExistsResult.IsFailure)
            return nameExistsResult.Error.ToErrors();

        if (nameExistsResult.Value)
            return PositionErrors.ActiveNameAlreadyExists().ToErrors();

        var name = PositionName.Create(
            command.Request.PositionName.Speciality,
            command.Request.PositionName.Direction).Value;

        var renameResult = position.Rename(name);
        if (renameResult.IsFailure)
            return renameResult.Error.ToErrors();

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

        _logger.LogInformation("Renamed position {PositionId}", position.Id);

        return position.Id;
    }
}
