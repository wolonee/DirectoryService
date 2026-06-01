using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.Commands.DeletePosition;

public record DeletePositionCommand(Guid PositionId) : ICommand;
