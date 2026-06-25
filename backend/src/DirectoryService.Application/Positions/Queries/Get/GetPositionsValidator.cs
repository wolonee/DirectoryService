using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Common;
using DirectoryService.Shared.EntitiesErrors;
using FluentValidation;

namespace DirectoryService.Application.Positions.Queries.Get;

public class GetPositionsValidator : AbstractValidator<GetPositionsQuery>
{
    public GetPositionsValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetPositionsQuery.Request)));

        RuleFor(x => x.Request.Search)
            .MaximumLength(500)
            .WithError(GeneralErrors.MaximumLength(500, nameof(GetPositionsQuery.Request.Search)));

        RuleFor(x => x.Request.SortBy)
            .Must(x => x == null || x.ToLower() == "speciality" || x.ToLower() == "direction" || x.ToLower() == "created_at")
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetPositionsQuery.Request.SortBy)));

        RuleFor(x => x.Request.SortDir)
            .Must(x => x == null || x.ToLower() == "asc" || x.ToLower() == "desc")
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetPositionsQuery.Request.SortDir)));

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
