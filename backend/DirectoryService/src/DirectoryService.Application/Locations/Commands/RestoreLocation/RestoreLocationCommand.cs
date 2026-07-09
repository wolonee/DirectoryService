using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Commands.RestoreLocation;

public record RestoreLocationCommand(Guid LocationId) : ICommand;
