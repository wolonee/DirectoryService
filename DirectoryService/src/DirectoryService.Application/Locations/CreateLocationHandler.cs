
using CSharpFunctionalExtensions;
using DirectoryService.Presentation.Controllers;

namespace DirectoryService.Application.Locations;



public class CreateLocationHandler
{
    
    public CreateLocationHandler()
    {
    }
    
    public static async Task<Result<Guid>> Handle(CreateLocationAddressDto dto, CancellationToken ct = default)
    {
        // валидация входных данных
        // бизнес валидация
        // создание сущности Location
        // Сохранение сущности Location в базе данных
        // Логирование об успешном сохранении
    }
}