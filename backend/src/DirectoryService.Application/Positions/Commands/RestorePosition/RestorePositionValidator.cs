using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Positions.Commands.RestorePosition;

public class RestorePositionValidator : AbstractValidator<RestorePositionCommand>
{
    public RestorePositionValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("positionId"));
    }
}
