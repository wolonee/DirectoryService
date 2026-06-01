using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Commands.DeleteLocation;

public class DeleteLocationHandler : ICommandHandler<DeleteLocationCommand>
{
    private readonly IValidator<DeleteLocationCommand> _validator;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteLocationHandler> _logger;

    public DeleteLocationHandler(
        IValidator<DeleteLocationCommand> validator,
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteLocationHandler> logger)
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        DeleteLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Delete Location Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var locationResult = await _locationsRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var location = locationResult.Value;

        var deleteLinksResult = await _locationsRepository.DeleteDepartmentLocationsByLocationId(
            command.LocationId,
            cancellationToken);

        if (deleteLinksResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteLinksResult.Error.ToErrors();
        }

        var deactivateResult = location.Deactivate();
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

        _logger.LogInformation("Deleted location {LocationId}", command.LocationId);

        return UnitResult.Success<Errors>();
    }
}
