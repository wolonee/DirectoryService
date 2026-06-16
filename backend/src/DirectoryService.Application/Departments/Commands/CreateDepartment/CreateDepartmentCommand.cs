using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest request) : ICommand;