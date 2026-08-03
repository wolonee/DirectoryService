using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Positions.Commands.SoftDeletePosition;

public class DeletePositionValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("positionId"));
    }
}
