using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Commands.Photo.DeletePhoto;

public class DeletePhotoHandler : ICommandHandler<DeletePhotoCommand>
{
    private readonly ITransactionManager _transactionManager;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<DeletePhotoHandler> _logger;

    public DeletePhotoHandler(
        ITransactionManager transactionManager,
        ILocationsRepository locationsRepository,
        ILogger<DeletePhotoHandler> logger)
    {
        _transactionManager = transactionManager;
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(DeletePhotoCommand command, CancellationToken cancellationToken)
    {
        var locationId = command.LocationId;

        if (locationId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid().ToErrors();
        
        var locationResult = await _locationsRepository.GetByIdAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            _logger.LogWarning("Could not delete photo because location {LocationId} was not found", locationId);
            return locationResult.Error.ToErrors();
        }
        
        var location = locationResult.Value;
        
        UnitResult<Error> removeResult = location.RemovePhoto();
        if (removeResult.IsFailure)
        {
            _logger.LogWarning(
                "Could not remove photo from location {LocationId}. Error: {@Error}",
                locationId,
                removeResult.Error);

            return removeResult.Error.ToErrors();
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            _logger.LogError(
                "Failed to delete the photo relation from location {LocationId}. Error: {@Error}",
                locationId,
                saveChangesResult.Error);

            return saveChangesResult.Error.ToErrors();
        }

        _logger.LogInformation("Photo relation was deleted from location {LocationId}", locationId);

        return UnitResult.Success<Errors>();
    }
}
