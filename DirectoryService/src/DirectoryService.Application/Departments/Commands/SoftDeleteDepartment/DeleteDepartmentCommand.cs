using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid DepartmentId) : ICommand;
