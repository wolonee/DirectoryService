using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public record DeleteDepartmentCommand(Guid DepartmentId) : ICommand;
