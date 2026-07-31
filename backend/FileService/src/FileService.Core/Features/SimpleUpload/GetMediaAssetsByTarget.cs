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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FileService.Core.Features.SimpleUpload;

public sealed class GetMediaAssetsByTargetValidator : AbstractValidator<GetMediaAssetsByTargetQuery>
{
    public GetMediaAssetsByTargetValidator()
    {
        RuleFor(query => query.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetMediaAssetsByTargetQuery.Request)));

        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.TargetId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired(nameof(GetMediaAssetsByTargetRequest.TargetId)));
        });
    }
}

public sealed class GetMediaAssetsByTargetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files", async Task<EndpointResult<GetMediaAssetsByTargetResponse>>(
            [AsParameters] GetMediaAssetsByTargetRequest request,
            [FromServices] GetMediaAssetsByTargetHandler handler,
            CancellationToken cancellationToken) => 
        {
            var query = new GetMediaAssetsByTargetQuery(request);
            
            return await handler.Handle(query, cancellationToken);
        });
    }
}

public sealed class GetMediaAssetsByTargetHandler
    : IQueryHandler<GetMediaAssetsByTargetResponse, GetMediaAssetsByTargetQuery>
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetMediaAssetsByTargetQuery> _validator;

    public GetMediaAssetsByTargetHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext,
        IValidator<GetMediaAssetsByTargetQuery> validator)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
        _validator = validator;
    }

    public async Task<Result<GetMediaAssetsByTargetResponse, Errors>> Handle(
        GetMediaAssetsByTargetQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        var request = query.Request;

        IQueryable<MediaAsset> mediaAssetsQuery = _readDbContext.MediaAssetsQuery
            .Where(asset => 
                asset.Owner.EntityId == request.TargetId
                && asset.Status != MediaStatus.DELETED);

        if (!string.IsNullOrWhiteSpace(request.TargetType))
            mediaAssetsQuery = mediaAssetsQuery.Where(asset => asset.Owner.Context == request.TargetType);

        List<MediaAsset> mediaAssets = await mediaAssetsQuery
            .ToListAsync(cancellationToken);
        
        List<MediaAsset> readyMediaAssets = mediaAssets.Where(a => a.Status == MediaStatus.READY).ToList();
        List<StorageKey> keys = readyMediaAssets.Select(a => a.FinalKey).ToList();

        var urlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keys, cancellationToken);
        if (urlsResult.IsFailure)
            return urlsResult.Error.ToErrors();
        
        var urls = urlsResult.Value;
        
        var urlsDict = urls.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);

        var mediaAssetDtoList = new List<GetMediaAssetByTargetDto>();
        foreach (var mediaAsset in mediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.FinalKey, out string? url);

            var dto = new GetMediaAssetByTargetDto(
                mediaAsset.Id,
                mediaAsset.Owner.EntityId,
                mediaAsset.Owner.Context,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                mediaAsset.MediaData.ContentType.Value.ToLowerInvariant(),
                url);
            
            mediaAssetDtoList.Add(dto);
        }
        
        var response = new GetMediaAssetsByTargetResponse
        {
            TargetId = request.TargetId,
            TargetType = request.TargetType,
            Files = mediaAssetDtoList,
        };

        return response;
    }

}











