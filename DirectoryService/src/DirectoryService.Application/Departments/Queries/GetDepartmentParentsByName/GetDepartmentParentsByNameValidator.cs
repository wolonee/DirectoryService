using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Common;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentParentsByName;

public class GetDepartmentParentsByNameValidator : AbstractValidator<GetDepartmentParentsByNameQuery>
{
    public GetDepartmentParentsByNameValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired());
        
        RuleFor(x => x.Request.Name)
            .MinimumLength(2)
            .WithError(GeneralErrors.MinimumLength(2, "name"));
        
        When(x => x.Request.Pagination != null, () =>
        {
            RuleFor(x => x.Request.Pagination!.Page)
                .GreaterThanOrEqualTo(1)
                .WithError(GeneralErrors.MinimumLength(1, nameof(PaginationRequest)));
            
            RuleFor(x => x.Request.Pagination!.PageSize)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(100)
                .WithError(GeneralErrors.ValueHasBoundedLength(1, 100, nameof(PaginationRequest)));
        });
    }
}