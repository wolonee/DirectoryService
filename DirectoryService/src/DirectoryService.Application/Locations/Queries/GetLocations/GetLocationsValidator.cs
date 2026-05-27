using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationsValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetLocationsQuery.Request)));
        
        RuleFor(q => q.Request.Search)
            .MaximumLength(1000)
            .WithError(GeneralErrors.MaximumLength(1000, nameof(GetLocationsQuery.Request.Search)));
        
        RuleFor(q => q.Request.Country)
            .MaximumLength(500)
            .WithError(GeneralErrors.MaximumLength(500, nameof(GetLocationsQuery.Request.Country)));
        
        RuleFor(q => q.Request.City)
            .MaximumLength(500)
            .WithError(GeneralErrors.MaximumLength(500, nameof(GetLocationsQuery.Request.City)));
        
        RuleFor(q => q.Request.Street)
            .MaximumLength(500)
            .WithError(GeneralErrors.MaximumLength(500, nameof(GetLocationsQuery.Request.Street)));
    }
}