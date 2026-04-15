using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
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