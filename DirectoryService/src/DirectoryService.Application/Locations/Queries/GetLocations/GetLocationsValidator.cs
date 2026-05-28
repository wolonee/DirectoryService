using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Common;
using DirectoryService.Shared;
using DirectoryService.Shared.EntitiesErrors;
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
        
        RuleFor(q => q.Request.Pagination!.Page)
            .GreaterThan(0)
            .When(q => q.Request.Pagination != null)
            .WithError(GeneralErrors.MinimumLength(1, nameof(PaginationRequest)));

        RuleFor(q => q.Request.Pagination!.PageSize)
            .GreaterThan(0)
            .When(q => q.Request.Pagination != null)
            .WithError(GeneralErrors.MinimumLength(1, nameof(PaginationRequest)));

        RuleFor(q => q.Request.DepartmentIds)
            .Must(ids => ids != null && ids.Distinct().Count() == ids.Length)
            .WithError(GeneralErrors.Duplicate(nameof(GetLocationsQuery.Request.DepartmentIds)));
    }
}