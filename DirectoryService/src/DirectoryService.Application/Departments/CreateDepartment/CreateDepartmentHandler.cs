using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        // validation
        
        
        // business validation 
        
        // Создание сущности Department
        
        // Сохранение в бд
        
        // логирование об успешном сохранении
    }
}