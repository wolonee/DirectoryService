using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;

    public CreateDepartmentHandler(IValidator<CreateDepartmentCommand> validator)
    {
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            validationResult.ToValidationErrors();
        }
        
        // business validation
        
        
        // Создание сущности Department
        
        // Сохранение в бд
        
        // логирование об успешном сохранении
    }
}