using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Contracts.HttpCommunication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Queries.GetLocationById;

public class GetLocationByIdHandler : IQueryHandler<GetLocationByIdResponse, GetLocationByIdQuery>
{
    private readonly IFileCommunicationService _fileCommunicationService;
    private readonly ILogger<GetLocationByIdHandler> _logger;
    private readonly IReadDbContext _readDbContext;

    public GetLocationByIdHandler(
        IFileCommunicationService fileCommunicationService,
        IReadDbContext readDbContext,
        ILogger<GetLocationByIdHandler> logger)
    {
        _fileCommunicationService = fileCommunicationService;
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<GetLocationByIdResponse, Errors>> Handle(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var location = await _readDbContext.LocationsRead
            .FirstOrDefaultAsync(location => location.Id == query.Id, cancellationToken);

        if (location is null)
            return LocationErrors.NotFound(query.Id).ToErrors();

        LocationPhotoResponse? photoResponse = null;

        if (location.Photo is not null)
        {
            var request = new GetMediaAssetRequest(location.Photo.AssetId);
            var assetResult = await _fileCommunicationService.GetMediaAsset(request, cancellationToken);

            string availability;
            string? contentUrl = null;
            string contentType = location.Photo.ContentType;

            if (assetResult.IsSuccess)
            {
                bool isAvailable = string.Equals(
                    assetResult.Value.Status,
                    "ready",
                    StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(assetResult.Value.ContentUrl);

                availability = isAvailable
                    ? LocationPhotoAvailability.Available.ToContractValue()
                    : LocationPhotoAvailability.TemporarilyUnavailable.ToContractValue();

                if (isAvailable)
                {
                    contentUrl = assetResult.Value.ContentUrl;
                    contentType = assetResult.Value.ContentType;
                }
            }
            else if (assetResult.Error.Any(error => error.Type == ErrorType.NOT_FOUND))
            {
                availability = LocationPhotoAvailability.Missing.ToContractValue();
            }
            else
            {
                availability = LocationPhotoAvailability.TemporarilyUnavailable.ToContractValue();
                _logger.LogWarning(
                    "Photo {AssetId} for location {LocationId} is temporarily unavailable",
                    location.Photo.AssetId,
                    location.Id);
            }

            photoResponse = new LocationPhotoResponse(
                location.Photo.AssetId,
                availability,
                contentType,
                contentUrl,
                location.Photo.AttachedAt);
        }

        return new GetLocationByIdResponse
        {
            Id = location.Id,
            Country = location.Address.Country,
            City = location.Address.City,
            Street = location.Address.Street,
            Name = location.Name.Value,
            Timezone = location.Timezone.Value,
            Photo = photoResponse,
        };
    }
}
