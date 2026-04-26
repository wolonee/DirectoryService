using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.UpdateParent;

public record UpdateParentCommand(Guid ParentId, UpdateParentRequest Request) : ICommand;