using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.Progress;
using FileService.Core.Abstractions;
using FileService.Core.Features;
using FileService.Domain.S3Entities.Assets;
using FileService.Domain.S3Entities.MediaProcessing;
using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.Progress;

public sealed record GetProgressByIdQuery(Guid FileId) : IQuery;

public class GetProgressByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{fileId:guid}/progress", async Task<EndpointResult<ProgressEventDto>> (
            [FromRoute] Guid fileId,
            [FromServices] IQueryHandler<ProgressEventDto, GetProgressByIdQuery> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProgressByIdQuery(fileId);

            return await handler.Handle(query, cancellationToken);
        });
    }
}

public class GetProgressByIdHandler : IQueryHandler<ProgressEventDto, GetProgressByIdQuery>
{
    private readonly IVideoProcessingRepository _videoProcessingRepository;
    private readonly IVideoAssetRepository _videoAssetRepository;

    public GetProgressByIdHandler(
        IVideoProcessingRepository videoProcessingRepository,
        IVideoAssetRepository videoAssetRepository)
    {
        _videoProcessingRepository = videoProcessingRepository;
        _videoAssetRepository = videoAssetRepository;
    }

    public async Task<Result<ProgressEventDto, Errors>> Handle(
        GetProgressByIdQuery query,
        CancellationToken cancellationToken)
    {
        // Снапшот читаем из БД — она источник истины и отвечает СРАЗУ, без ожидания.
        // Очередь трогать нельзя: это односторонний буфер, его дренит ProgressConsumer.
        Result<VideoProcess, Error> processResult = await _videoProcessingRepository.GetBy(
            vp => vp.VideoAssetId == query.FileId,
            cancellationToken);
        if (processResult.IsFailure)
            return processResult.Error.ToErrors();

        // Статус для клиента берётся от asset-а (см. ProgressEventMapper).
        Result<VideoAsset, Error> assetResult = await _videoAssetRepository.GetByIdAsync(
            query.FileId,
            cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();

        return ProgressEventMapper.ToDto(processResult.Value, assetResult.Value.Status);
    }
}
