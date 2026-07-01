using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Commands.CreateLocation;
using DirectoryService.Application.Locations.Commands.DeleteLocation;
using DirectoryService.Application.Locations.Commands.UpdateLocation;
using DirectoryService.Application.Locations.Queries.GetLocationById;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Locations;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    private readonly ILogger<Location> _logger;
    
    public LocationController(ILogger<Location> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand(request);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpGet]
    public async Task<EndpointResult<PaginationResponse<GetLocationDto>>> Get(
        [FromQuery] GetLocationsRequest request,
        [FromServices] IQueryHandler<PaginationResponse<GetLocationDto>, GetLocationsQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery(request);
        
        return await handler.Handle(query, cancellationToken);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<EndpointResult<GetLocationByIdResponse>> GetById(
        [FromServices] IQueryHandler<GetLocationByIdResponse, GetLocationByIdQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationByIdQuery(id);
        
        return await handler.Handle(query, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<EndpointResult> Update(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<UpdateLocationCommand> handler,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLocationCommand(id, request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<EndpointResult> SoftDelete(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<DeleteLocationCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteLocationCommand(id);

        return await handler.Handle(command, cancellationToken);
    }
}