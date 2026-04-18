using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Positions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionController : ControllerBase
{

    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request)
    {
        var command = new CreatePositionCommand(request);
        
        return await handler.Handle(command);
    }
}