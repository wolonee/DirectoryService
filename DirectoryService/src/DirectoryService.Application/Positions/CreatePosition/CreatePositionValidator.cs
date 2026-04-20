using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(x => x.request.PositionName)
            .MustBeValueObject(name =>
                PositionName.Create(
                    name.Speciality,
                    name.Direction));

        RuleFor(x => x.request.Description)
            .MustBeValueObject(PositionDescription.Create);
        
        RuleFor(x => x.request.DepartmentIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("departmentIds"))
            .Must(ids => ids.Length == ids.Distinct().Count())
            .WithError(PositionErrors.HasDuplicatedDepartments());
    }
}