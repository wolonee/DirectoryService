using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Contracts.HttpCommunication;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Commands.Photo.UpdatePhoto;

public class UpdatePhotoHandler : ICommandHandler<UpdatePhotoCommand>
{
    private readonly IFileCommunicationService _fileCommunicationService;
    private readonly ITransactionManager _transactionManager;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<UpdatePhotoHandler> _logger;

    public UpdatePhotoHandler(
        IFileCommunicationService fileCommunicationService,
        ITransactionManager transactionManager,
        ILocationsRepository locationsRepository,
        ILogger<UpdatePhotoHandler> logger)
    {
        _fileCommunicationService = fileCommunicationService;
        _transactionManager = transactionManager;
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(UpdatePhotoCommand command, CancellationToken cancellationToken)
    {
        // check guids
        var locationId = command.LocationId;
        var assetId = command.Request.AssetId;
        
        if (locationId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("LocationId").ToErrors();
        
        if (assetId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("AssetId").ToErrors();
        
        // get location
        var locationResult = await _locationsRepository.GetByIdAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            _logger.LogWarning(
                "Could not replace the photo with asset {AssetId} because location {LocationId} was not found",
                assetId,
                locationId);

            return locationResult.Error.ToErrors();
        }

        var location = locationResult.Value;
        
        // get metadata
        var request = new GetMediaAssetRequest(assetId);
        Result<GetMediaAssetResponse, Errors> fileInfoResult = await _fileCommunicationService.GetMediaAsset(request, cancellationToken);
        if (fileInfoResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to validate asset {AssetId} before replacing the photo of location {LocationId}. Errors: {@Errors}",
                assetId,
                locationId,
                fileInfoResult.Error);

            return fileInfoResult.Error;
        }

        var fileInfo = fileInfoResult.Value;
        
        UnitResult<Error> policyResult = LocationPhotoPolicy.Validate(fileInfo, locationId);
        if (policyResult.IsFailure)
        {
            _logger.LogWarning(
                "Asset {AssetId} cannot replace the photo of location {LocationId}. Error: {@Error}",
                assetId,
                locationId,
                policyResult.Error);

            return policyResult.Error.ToErrors();
        }
        
        // create LocationPhoto
        var photoResult = LocationPhoto.Create(assetId, fileInfo.ContentType, DateTime.UtcNow);
        if (photoResult.IsFailure)
        {
            _logger.LogWarning(
                "Could not create replacement photo metadata for asset {AssetId} and location {LocationId}. Error: {@Error}",
                assetId,
                locationId,
                photoResult.Error);

            return photoResult.Error.ToErrors();
        }

        var photo = photoResult.Value;
        
        UnitResult<Error> replaceResult = location.ReplacePhoto(photo);
        if (replaceResult.IsFailure)
        {
            _logger.LogWarning(
                "Could not replace photo of location {LocationId} with asset {AssetId}. Error: {@Error}",
                locationId,
                assetId,
                replaceResult.Error);

            return replaceResult.Error.ToErrors();
        }

        // save changes async
        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            _logger.LogError(
                "Failed to save replacement photo asset {AssetId} for location {LocationId}. Error: {@Error}",
                assetId,
                locationId,
                saveChangesResult.Error);

            return saveChangesResult.Error.ToErrors();
        }

        _logger.LogInformation(
            "Photo of location {LocationId} was replaced with asset {AssetId}",
            locationId,
            assetId);

        return UnitResult.Success<Errors>();
    }
}
