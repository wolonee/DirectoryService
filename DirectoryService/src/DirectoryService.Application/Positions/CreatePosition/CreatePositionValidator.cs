using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(x => x.request.Name)
            .MustBeValueObject(name =>
                PositionName.Create(
                    name.Speciality,
                    name.Direction));
        
        
    }
}