using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateParent;

public class UpdateParentHandler : ICommandHandler<Guid, UpdateParentCommand>
{
    private readonly IValidator<UpdateParentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<UpdateParentHandler> _logger;

    public UpdateParentHandler(
        IValidator<UpdateParentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ILogger<UpdateParentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateParentCommand command, CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();
        
        // Business validation
        var departmentResult = await _departmentsRepository.GetActiveDepartmentAsync(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();
        
        var department = departmentResult.Value;
        
        if (department == null)
            return GeneralErrors.NotFound(null, "department").ToErrors();

        if (command.Request.ParentId == command.DepartmentId)
            return DepartmentErrors.ParentIdEqualDepartmentId().ToErrors();

        if (command.Request.ParentId != null)
        {
            var parentResult = await _departmentsRepository.GetActiveParentAsync(command.DepartmentId, cancellationToken);
            if (parentResult.IsFailure)
                return parentResult.Error.ToErrors();
        
            var parent = parentResult.Value;
        
            if (parent == null)
                return GeneralErrors.NotFound(null, "department").ToErrors();
        
            if (department.ChildrenDepartments.Contains(parent))
                return DepartmentErrors.DepartmentChildrensContainsParent().ToErrors();

            // Update parent
            var updateDepartmentResult = department.UpdateParent(parent);
            if (updateDepartmentResult.IsFailure)
                return updateDepartmentResult.Error.ToErrors();
        }
        else
        {
            // Update parent
            var updateDepartmentResult = department.UpdateNullParent();
            if (updateDepartmentResult.IsFailure)
                return updateDepartmentResult.Error.ToErrors();
        }
        
        // Save in database
        
        // Logging about success result
        _logger.LogInformation("Department was successfully updated.");
        
        return Guid.NewGuid();
    }
}