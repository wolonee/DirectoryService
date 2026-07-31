using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.SimpleUpload;

public sealed class GetMediaAssetValidator : AbstractValidator<GetMediaAssetQuery>
{
    public GetMediaAssetValidator()
    {
        RuleFor(query => query.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetMediaAssetQuery.FileId)));
    }
}

public sealed class GetMediaAssetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{fileId:guid}", async Task<EndpointResult<GetMediaAssetResponse>> (
            [FromRoute] Guid fileId,
            [FromServices] GetMediaAssetHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.Handle(new GetMediaAssetQuery(fileId), cancellationToken);
        });
    }
}

public sealed class GetMediaAssetHandler
    : IQueryHandler<GetMediaAssetResponse, GetMediaAssetQuery>
{
    private readonly IMediaAssetRepository _repository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<GetMediaAssetQuery> _validator;

    public GetMediaAssetHandler(
        IMediaAssetRepository repository,
        IS3Provider s3Provider,
        IValidator<GetMediaAssetQuery> validator)
    {
        _repository = repository;
        _s3Provider = s3Provider;
        _validator = validator;
    }

    public async Task<Result<GetMediaAssetResponse, Errors>> Handle(
        GetMediaAssetQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        Result<MediaAsset, Error> assetResult = await _repository.GetByIdAsync(query.FileId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();

        MediaAsset asset = assetResult.Value;

        if (asset.Status == MediaStatus.DELETED)
            return GeneralErrors.NotFound(query.FileId, "File").ToErrors();

        ObjectMetadataDto? storage = asset.StorageReference is null
            ? null
            : new ObjectMetadataDto(
                asset.StorageReference.Size,
                asset.StorageReference.ContentType,
                asset.StorageReference.ETag,
                asset.StorageReference.Checksum,
                asset.StorageReference.LastModified);

        if (asset.Status != MediaStatus.READY)
        {
            return new GetMediaAssetResponse(
                asset.Id,
                asset.Owner.EntityId,
                asset.Owner.Context,
                asset.Status.ToString().ToLowerInvariant(),
                asset.AssetType.ToString().ToLowerInvariant(),
                asset.MediaData.ContentType.Value,
                asset.Usage.ToString().ToLowerInvariant(),
                asset.MediaData.Size,
                storage,
                null);
        }

        if (asset.StorageReference is null || asset.FinalKey == StorageKey.None)
            return Error.Conflict("media-asset.storage-reference-missing", "Ready file has no storage reference.").ToErrors();

        Result<string, Error> urlResult = await _s3Provider.GenerateDownloadUrlAsync(asset.FinalKey);
        if (urlResult.IsFailure)
            return urlResult.Error.ToErrors();

        return new GetMediaAssetResponse(
            asset.Id,
            asset.Owner.EntityId,
            asset.Owner.Context,
            asset.Status.ToString().ToLowerInvariant(),
            asset.AssetType.ToString().ToLowerInvariant(),
            asset.MediaData.ContentType.Value,
            asset.Usage.ToString().ToLowerInvariant(),
            asset.MediaData.Size,
            storage,
            urlResult.Value);
    }
}
