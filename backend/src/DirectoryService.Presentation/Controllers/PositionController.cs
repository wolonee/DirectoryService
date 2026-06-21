using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.Commands.CreatePosition;
using DirectoryService.Application.Positions.Commands.DeletePosition;
using DirectoryService.Application.Positions.Commands.RenamePosition;
using DirectoryService.Contracts.Positions;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request)
    {
        var command = new CreatePositionCommand(request);
        
        return await handler.Handle(command);
    }

    [HttpPatch("{id:guid}")]
    public async Task<EndpointResult<Guid>> Rename(
        [FromRoute] Guid id,
        [FromBody] RenamePositionRequest request,
        [FromServices] ICommandHandler<Guid, RenamePositionCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new RenamePositionCommand(id, request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<EndpointResult> SoftDelete(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<DeletePositionCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DeletePositionCommand(id);

        return await handler.Handle(command, cancellationToken);
    }
}