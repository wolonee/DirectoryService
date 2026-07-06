using DirectoryService.Application.Validation;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Locations.Commands.RestoreLocation;

public class RestoreLocationValidator : AbstractValidator<RestoreLocationCommand>
{
    public RestoreLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("locationId"));
    }
}
