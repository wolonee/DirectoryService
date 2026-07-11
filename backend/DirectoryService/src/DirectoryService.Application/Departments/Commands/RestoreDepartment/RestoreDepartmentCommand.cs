using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.RestoreDepartment;

public record RestoreDepartmentCommand(Guid DepartmentId) : ICommand;
