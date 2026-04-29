using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.UpdateParent;

public record UpdateParentCommand(Guid DepartmentId, UpdateParentRequest Request) : ICommand;