using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Locations.Commands.SoftDeleteLocation;

public class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("locationId"));
    }
}
