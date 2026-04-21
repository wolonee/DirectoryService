using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateDepartmentRequest request,
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDepartmentCommand(request);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    // PUT /api/departments/{departmentId}/locations

    [HttpPut("{departmentId:guid}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
        [FromServices] ICommandHandler<Guid, UpdateLocationsCommand> handler,
        [FromRoute] Guid departmentId,
        [FromBody] UpdateLocationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLocationsCommand(departmentId, request);
        
        return await handler.Handle(command, cancellationToken);
    }
}