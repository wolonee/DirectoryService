using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    public Task<Result<Guid, Errors>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        // validation
        
        // business validation
        
        // create position
        
        // save position in db
        
        // logger about success save
    }
}