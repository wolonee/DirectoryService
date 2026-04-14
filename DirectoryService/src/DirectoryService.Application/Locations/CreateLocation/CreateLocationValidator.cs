using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            // .MustBeValueObject(LocationName.Create)
            
            .MaximumLength(LengthConstants.LENGTH120)
            .WithErrorCode("name.too.long")
            .WithMessage("Name must not exceed 120 characters.")
            
            .MinimumLength(LengthConstants.LENGTH3)
            .WithErrorCode("name.too.short")
            .WithMessage("Name must be more than 3 characters.");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithErrorCode("street.is.empty")
            .WithMessage("Street is required.");
        
        RuleFor(x => x.City)
            .NotEmpty()
            .WithErrorCode("city.is.empty")
            .WithMessage("City is required.");
        
        RuleFor(x => x.Country)
            .NotEmpty()
            .WithErrorCode("country.is.empty")
            .WithMessage("Country is required.");
        
        RuleFor(x => x.Timezone)
            .NotEmpty()
            .Must(BeValidTimeZone)
            .WithErrorCode("timezone.is.invalid")
            .WithMessage("Time zone must be valid IANA format");
    }

    private bool BeValidTimeZone(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            return false;

        if (!timezone.Contains('/'))
            return false;
        
        return TimeZoneInfo.TryFindSystemTimeZoneById(timezone, out _);
    }
}