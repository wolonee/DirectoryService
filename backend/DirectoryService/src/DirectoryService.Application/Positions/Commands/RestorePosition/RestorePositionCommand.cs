using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.Commands.RestorePosition;

public record RestorePositionCommand(Guid PositionId) : ICommand;
