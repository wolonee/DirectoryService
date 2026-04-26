using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments.UpdateParent;

public class UpdateParentHandler : ICommandHandler<Guid, UpdateParentCommand>
{
    public Task<Result<Guid, Errors>> Handle(UpdateParentCommand command, CancellationToken cancellationToken = default)
    {
        // Validation
        
        // Business validation
        
        // Update parent
        
        // Save in database

        // Logging about success result
    }
}