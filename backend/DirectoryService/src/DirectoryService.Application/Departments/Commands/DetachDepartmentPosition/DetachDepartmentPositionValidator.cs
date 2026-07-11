using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.DetachDepartmentPosition;

public class DetachDepartmentPositionValidator : AbstractValidator<DetachDepartmentPositionCommand>
{
    public DetachDepartmentPositionValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("departmentId"));

        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("positionId"));
    }
}
