using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.Simple.AssetExists;
using FileService.Core.Abstractions;
using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.Simple;

public sealed record AssetExistsQuery(AssetExistsRequest Request) : IQuery;

public sealed class AssetExistsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{fileId:guid}/exists", async Task<EndpointResult<AssetExistsResponse>> (
                [FromRoute] Guid fileId,
                [FromServices] IQueryHandler<AssetExistsResponse, AssetExistsQuery> handler,
                CancellationToken cancellationToken) =>
            {
                var request = new AssetExistsRequest(fileId);
                var query = new AssetExistsQuery(request);

                return await handler.Handle(query, cancellationToken);
            });
    }
}

public sealed class AssetExistsHandler : IQueryHandler<AssetExistsResponse, AssetExistsQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AssetExistsHandler> _logger;

    public AssetExistsHandler(
        IReadDbContext readDbContext,
        ILogger<AssetExistsHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<AssetExistsResponse, Errors>> Handle(AssetExistsQuery query, CancellationToken cancellationToken)
    {
        var assetId = query.Request.FileId;

        if (assetId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("id").ToErrors();
        
        bool result = await _readDbContext.MediaAssetsQuery.AnyAsync(x => x.Id == assetId, cancellationToken);
        
        return new AssetExistsResponse(result);
    }
}
