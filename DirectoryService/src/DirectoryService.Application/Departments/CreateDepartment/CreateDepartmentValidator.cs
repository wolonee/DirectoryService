using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(x => x.request.Name)
            .MustBeValueObject(DepartmentName.Create);
        
        RuleFor(x => x.request.Identifier)
            .MustBeValueObject(DepartmentIdentifier.Create);
    }
}