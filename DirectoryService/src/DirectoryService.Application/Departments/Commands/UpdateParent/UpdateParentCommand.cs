using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Commands.UpdateParent;

public record UpdateParentCommand(Guid DepartmentId, UpdateParentRequest Request) : ICommand;