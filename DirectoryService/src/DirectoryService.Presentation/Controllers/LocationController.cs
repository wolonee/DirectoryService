using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Application.Locations.Query;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<EndpointResult<GetLocationsResponse>> Get(
        [FromServices] IQueryHandler<GetLocationsResponse, GetLocationsQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery();
        
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
}