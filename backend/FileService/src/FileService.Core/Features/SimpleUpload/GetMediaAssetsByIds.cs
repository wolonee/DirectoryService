using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FileService.Core.Features.SimpleUpload;

public sealed class GetMediaAssetsValidator : AbstractValidator<GetMediaAssetsQuery>
{
    public GetMediaAssetsValidator()
    {
        RuleFor(query => query.FileIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetMediaAssetsQuery.FileIds)));
    }
}

public sealed class GetMediaAssetsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files", async Task<EndpointResult<GetMediaAssetsResponse>> (
            [FromBody] GetMediaAssetsQuery query,
            [FromServices] GetMediaAssetsHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.Handle(query, cancellationToken);
        });
    }
}

public sealed class GetMediaAssetsHandler
    : IQueryHandler<GetMediaAssetsResponse, GetMediaAssetsQuery>
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetMediaAssetsQuery> _validator;

    public GetMediaAssetsHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext,
        IValidator<GetMediaAssetsQuery> validator)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
        _validator = validator;
    }

    public async Task<Result<GetMediaAssetsResponse, Errors>> Handle(GetMediaAssetsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        var fileIds = query.FileIds;

        if (!fileIds.Any())
            return new GetMediaAssetsResponse([]);

        List<MediaAsset> mediaAssets = await _readDbContext.MediaAssetsQuery
            .Where(x => fileIds.Contains(x.Id) && x.Status != MediaStatus.DELETED)
            .ToListAsync(cancellationToken: cancellationToken);

        List<MediaAsset> readyMediaAssets = mediaAssets.Where(x => x.Status == MediaStatus.READY).ToList();
        List<StorageKey> keys = readyMediaAssets.Select(x => x.FinalKey).ToList();
        
        Result<MediaUrl[], Error> urlResult = await _s3Provider.GenerateDownloadUrlsAsync(keys, cancellationToken);
        if (urlResult.IsFailure)
            return urlResult.Error.ToErrors();
        
        var urls = urlResult.Value;
        
        var urlsDict = urls.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);

        var response = new List<GetMediaAssetDto>();
        foreach (var mediaAsset in mediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.FinalKey, out string? url);

            var res = new GetMediaAssetDto(
                mediaAsset.Id,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                mediaAsset.MediaData.ContentType.Value.ToLowerInvariant(),
                url);
            
            response.Add(res);
        }

        return new GetMediaAssetsResponse(response.ToArray());
    }

}
