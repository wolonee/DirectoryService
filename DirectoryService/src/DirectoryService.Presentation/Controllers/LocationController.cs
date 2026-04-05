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
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationAddressDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(request, cancellationToken);
        
        return Ok(result);
    }
}