using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.DetachDepartmentPosition;

public record DetachDepartmentPositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;
