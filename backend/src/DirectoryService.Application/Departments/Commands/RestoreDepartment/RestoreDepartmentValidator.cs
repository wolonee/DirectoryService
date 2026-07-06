using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.RestoreDepartment;

public class RestoreDepartmentValidator : AbstractValidator<RestoreDepartmentCommand>
{
    public RestoreDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("departmentId"));
    }
}
