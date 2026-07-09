using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Locations.Commands.UpdateLocation;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("locationId"));

        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(x => x.Request.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(x => x.Request.Address)
            .MustBeValueObject(address =>
                LocationAddress.Create(
                    address.Street,
                    address.City,
                    address.Country));

        RuleFor(x => x.Request.Timezone)
            .MustBeValueObject(LocationTimeZone.Create);
    }
}
