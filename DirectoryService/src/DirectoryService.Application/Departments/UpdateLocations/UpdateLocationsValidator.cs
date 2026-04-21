using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationsCommand>
{
    public UpdateLocationsValidator()
    {
        RuleFor(x => x.departmentId)
            .NotEmpty()
            .WithError(GeneralErrors.NotFound(null, "departmentId"));

        RuleFor(x => x.request.LocationsIds)
            .NotEmpty()
            .WithError(GeneralErrors.NotFound(null, "locationsIds"))
            .Must(locationsIds => locationsIds.Length == locationsIds.Distinct().Count())
            .WithError(DepartmentErrors.HasDuplicatedLocations());
    }
}