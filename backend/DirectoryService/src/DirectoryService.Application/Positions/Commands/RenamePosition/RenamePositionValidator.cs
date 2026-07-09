using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Positions.Commands.RenamePosition;

public class RenamePositionValidator : AbstractValidator<RenamePositionCommand>
{
    public RenamePositionValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("positionId"));

        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(x => x.Request.PositionName)
            .MustBeValueObject(name =>
                PositionName.Create(
                    name.Speciality,
                    name.Direction));
    }
}
