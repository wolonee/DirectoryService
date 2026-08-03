using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Commands.SoftDeleteLocation;

public record DeleteLocationCommand(Guid LocationId) : ICommand;
