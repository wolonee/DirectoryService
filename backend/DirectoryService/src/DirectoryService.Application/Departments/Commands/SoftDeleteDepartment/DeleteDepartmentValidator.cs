using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("departmentId"));
    }
}
