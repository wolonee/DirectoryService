using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.UpdateParent;

public class UpdateParentHandler : ICommandHandler<UpdateParentCommand>
{
    private readonly IValidator<UpdateParentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly HybridCache _cache;
    private readonly ILogger<UpdateParentHandler> _logger;

    public UpdateParentHandler(
        IValidator<UpdateParentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        HybridCache cache,
        ILogger<UpdateParentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(UpdateParentCommand command, CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Update Parent Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }
        
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;
        
        // Business validation
        var departmentResult = await _departmentsRepository.GetActiveDepartmentWithLock(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();
        
        var department = departmentResult.Value;
        
        if (department.IsDeleted)
            return DepartmentErrors.IsDeleted().ToErrors();

        if (command.Request.ParentId == command.DepartmentId)
            return DepartmentErrors.ParentIdEqualDepartmentId().ToErrors();

        if (command.Request.ParentId == department.ParentId)
            return DepartmentErrors.AlreadyHasThisParent().ToErrors();

        var lockDescendantsResult = await _departmentsRepository.LockDescendants(department.DepartmentPath.Value, cancellationToken);
        if (lockDescendantsResult.IsFailure)
            return lockDescendantsResult.Error.ToErrors();
        
        string rootPath = department.DepartmentPath.Value;
        string newParentPath = string.Empty;
        
        if (command.Request.ParentId != null)
        {
            var parentResult = await _departmentsRepository.GetActiveDepartmentWithLock(command.Request.ParentId.Value, cancellationToken);
            if (parentResult.IsFailure)
                return parentResult.Error.ToErrors();
        
            var parent = parentResult.Value;
        
            if (department.ChildrenDepartments.Contains(parent))
                return DepartmentErrors.DepartmentChildrensContainsParent().ToErrors();

            newParentPath = parent.DepartmentPath.Value;

            // Update parent
            var updateParentResult = await _departmentsRepository.UpdateParent(
                rootPath,
                newParentPath,
                department.Id,
                parent.Id,
                cancellationToken);
            if (updateParentResult.IsFailure)
                return updateParentResult.Error.ToErrors();
        }
        else
        {
            // Update parent
            var updateParentResult = await _departmentsRepository.UpdateParent(
                rootPath,
                newParentPath,
                department.Id,
                null,
                cancellationToken);
            if (updateParentResult.IsFailure)
                return updateParentResult.Error.ToErrors();
        }
        
        // Save in database
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

        // Инвалидация после коммита: гасим старого и нового родителя (null → no-op внутри)
        await InvalidateChildren(department.ParentId, cancellationToken);
        await InvalidateChildren(command.Request.ParentId, cancellationToken);

        // Logging about success result
        _logger.LogInformation("Department was successfully updated.");
        
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