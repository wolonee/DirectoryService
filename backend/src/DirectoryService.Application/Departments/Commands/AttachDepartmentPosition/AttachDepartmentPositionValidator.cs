using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.AttachDepartmentPosition;

public class AttachDepartmentPositionValidator : AbstractValidator<AttachDepartmentPositionCommand>
{
    public AttachDepartmentPositionValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("departmentId"));

        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("positionId"));
    }
}
