using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.AttachDepartmentPosition;

public record AttachDepartmentPositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;
