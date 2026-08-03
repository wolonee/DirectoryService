using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.Commands.SoftDeletePosition;

public record DeletePositionCommand(Guid PositionId) : ICommand;
