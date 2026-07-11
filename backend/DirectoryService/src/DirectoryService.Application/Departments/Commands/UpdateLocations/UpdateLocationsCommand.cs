using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Commands.UpdateLocations;

public record UpdateLocationsCommand(Guid departmentId, UpdateLocationsRequest request) : ICommand;