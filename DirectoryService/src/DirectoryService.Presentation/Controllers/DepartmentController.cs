using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Departments.Commands.UpdateLocations;
using DirectoryService.Application.Departments.Commands.UpdateParent;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
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
    
    // PUT /api/departments/{departmentId}/parent
    [HttpPut("{departmentId:guid}/parent")]
    public async Task<EndpointResult> UpdateLocations(
        [FromServices] ICommandHandler<UpdateParentCommand> handler,
        [FromRoute] Guid departmentId,
        [FromBody] UpdateParentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateParentCommand(departmentId, request);

        return await handler.Handle(command, cancellationToken);
    }
    
    // GET /api/departments/top-positions
    [HttpGet("top-positions")]
    public async Task<EndpointResult<GetTopDepartmentsByPositionsResponse>> GetByTopPositions(
        [FromServices] IQueryHandler<GetTopDepartmentsByPositionsResponse> handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(cancellationToken);
    }
}