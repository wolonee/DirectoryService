using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.Commands.RenamePosition;

public record RenamePositionCommand(Guid PositionId, RenamePositionRequest Request) : ICommand;
