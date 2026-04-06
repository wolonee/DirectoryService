using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
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
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand(request);
        
        var result = await handler.Handle(command, cancellationToken);
        
        return Ok(result);
    }
}