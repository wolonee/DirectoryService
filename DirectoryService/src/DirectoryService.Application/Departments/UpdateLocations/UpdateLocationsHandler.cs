using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
{
    public Task<Result<Guid, Errors>> Handle(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken = default)
    {
        // FluentValidation
        
        // Business validation
        
        // Update Locations
        
        // Save in database
        
        // Logging about success result
    }
}