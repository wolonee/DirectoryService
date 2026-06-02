using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Queries.Get;

public class GetDepartmentsValidator : AbstractValidator<GetDepartmentsQuery>
{
    public GetDepartmentsValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetDepartmentsQuery.Request)));
        
        RuleFor(x => x.Request.Search)
            .MaximumLength(500)
            .WithError(GeneralErrors.MaximumLength(500, nameof(GetDepartmentsQuery.Request.Search)));
        
        RuleFor(x => x.Request.SortBy)
            .Must(x => x == null || x.ToLower() == "name" || x.ToLower() == "created_at")
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetDepartmentsQuery.Request.SortBy)));
        
        RuleFor(x => x.Request.SortDir)
            .Must(x => x == null || x.ToLower() == "asc" || x.ToLower() == "desc")
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetDepartmentsQuery.Request.SortDir)));
        
        RuleFor(x => x.Request.Pagination!.Page)
            .GreaterThanOrEqualTo(0)
            .When(q => q.Request.Pagination != null)
            .WithError(GeneralErrors.MinimumLength(1, nameof(GetDepartmentsQuery.Request.Pagination.Page)));
        
        RuleFor(x => x.Request.Pagination!.PageSize)
            .GreaterThanOrEqualTo(0)
            .When(q => q.Request.Pagination != null)
            .WithError(GeneralErrors.MinimumLength(1, nameof(GetDepartmentsQuery.Request.Pagination.Page)))
            .LessThanOrEqualTo(100)
            .WithError(GeneralErrors.MaximumLength(100, nameof(GetDepartmentsQuery.Request.Pagination.Page)));

    }
}