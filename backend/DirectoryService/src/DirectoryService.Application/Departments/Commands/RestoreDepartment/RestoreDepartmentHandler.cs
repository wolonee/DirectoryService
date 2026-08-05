using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.RestoreDepartment;

public class RestoreDepartmentHandler : ICommandHandler<RestoreDepartmentCommand>
{
    private readonly IValidator<RestoreDepartmentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly HybridCache _cache;
    private readonly ILogger<RestoreDepartmentHandler> _logger;

    public RestoreDepartmentHandler(
        IValidator<RestoreDepartmentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        HybridCache cache,
        ILogger<RestoreDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        RestoreDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Restore Department Failed: {Error}", validationResult.ToValidationErrors());
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

        department.Restore();

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
        
        await InvalidateChildren(department.ParentId, cancellationToken);

        _logger.LogInformation("Restored department {DepartmentId}", command.DepartmentId);

        return UnitResult.Success<Errors>();
    }
    
    private async Task InvalidateChildren(Guid? parentId, CancellationToken ct)
    {
        if (parentId is null)
            return;

        try
        {
            await _cache.RemoveByTagAsync(DepartmentCacheKeys.ChildrenTag(parentId.Value), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to evict children cache for parent {ParentId}; it will expire by TTL",
                parentId.Value);
        }
    }
}
