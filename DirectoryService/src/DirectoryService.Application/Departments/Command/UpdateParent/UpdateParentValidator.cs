using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.UpdateParent;

public class UpdateParentValidator : AbstractValidator<UpdateParentCommand>
{
    public UpdateParentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.NotFound(null, "departmentId"));
    }
}