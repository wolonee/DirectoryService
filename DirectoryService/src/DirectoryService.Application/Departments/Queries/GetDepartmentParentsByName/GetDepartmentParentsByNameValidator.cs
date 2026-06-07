using DirectoryService.Application.Validation;
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
    }
}