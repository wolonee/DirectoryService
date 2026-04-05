using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(LengthConstants.LENGTH120).WithMessage("Name must not exceed 120 characters.")
            .MinimumLength(LengthConstants.LENGTH3).WithMessage("Name must be more than 3 characters.");

        RuleFor(x => x.Street).NotEmpty().WithMessage("Street is required.");
        
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required.");
        
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required.");
        
        RuleFor(x => x.Timezone).NotEmpty().WithMessage("Timezone is required.");
    }
}