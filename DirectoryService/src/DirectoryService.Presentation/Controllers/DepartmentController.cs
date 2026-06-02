using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.AttachDepartmentPosition;
using DirectoryService.Application.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Departments.Commands.DeleteDepartment;
using DirectoryService.Application.Departments.Commands.DetachDepartmentPosition;
using DirectoryService.Application.Departments.Commands.UpdateLocations;
using DirectoryService.Application.Departments.Commands.UpdateParent;
using DirectoryService.Application.Departments.Queries.Get;
using DirectoryService.Application.Departments.Queries.GetById;
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
    
    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult> Delete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<DeleteDepartmentCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteDepartmentCommand(departmentId);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("{departmentId:guid}/positions/{positionId:guid}")]
    public async Task<EndpointResult> AttachPosition(
        [FromRoute] Guid departmentId,
        [FromRoute] Guid positionId,
        [FromServices] ICommandHandler<AttachDepartmentPositionCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new AttachDepartmentPositionCommand(departmentId, positionId);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{departmentId:guid}/positions/{positionId:guid}")]
    public async Task<EndpointResult> DetachPosition(
        [FromRoute] Guid departmentId,
        [FromRoute] Guid positionId,
        [FromServices] ICommandHandler<DetachDepartmentPositionCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DetachDepartmentPositionCommand(departmentId, positionId);

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
    
    [HttpGet("{id:guid}")]
    public async Task<EndpointResult<GetByDepartmentIdDto>> GetById(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetByDepartmentIdDto, GetDepartmentByIdQuery> handler)
    {
        var query = new GetDepartmentByIdQuery(id);
        
        return await handler.Handle(query);
    }

    [HttpGet]
    public async Task<EndpointResult<GetDepartmentsResponse>> GetDepartments(
        [FromQuery] GetDepartmentsRequest request,
        [FromServices] IQueryHandler<GetDepartmentsResponse, GetDepartmentsQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentsQuery(request);
        
        return await handler.Handle(query, cancellationToken);
    }
}