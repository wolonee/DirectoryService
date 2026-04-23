using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Decorators;

public class LocationsRepositoryDecorator : ILocationsRepository
{
    private readonly LocationsRepository _innerRepo;
    private readonly ILogger<LocationsRepositoryDecorator> _logger;

    public LocationsRepositoryDecorator(LocationsRepository innerRepo, ILogger<LocationsRepositoryDecorator> logger)
    {
        _innerRepo = innerRepo;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        try
        {
            var addLocationResult = await _innerRepo.AddAsync(location, cancellationToken);
            if (addLocationResult.IsFailure)
                return addLocationResult.Error;
            
            return addLocationResult;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return LocationErrors.NameConflict(location.Name.Value);
            }

            _logger.LogError(ex, "Database update error while AddAsync location with id {Id}", location.Id);
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while AddAsync location with id {Id}", location.Id);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while AddAsync location with id {Id}", location.Id);
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var nameExistsResult = await _innerRepo.NameExistsAsync(name, cancellationToken: cancellationToken);
            if (nameExistsResult.IsFailure)
                return nameExistsResult.Error;
            
            return nameExistsResult;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while NameExistsAsync in location with name {Name}", name);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while NameExistsAsync in location with name {Name}", name);
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> AddressExistsAsync(CreateLocationAddressRequest address, CancellationToken cancellationToken = default)
    {
        try
        {
            var addressExistsResult = await _innerRepo.AddressExistsAsync(address, cancellationToken: cancellationToken);
            if (addressExistsResult.IsFailure)
                return addressExistsResult.Error;
            
            return addressExistsResult;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while AddressExistsAsync in location with address {Address}", address.ToString());
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while AddressExistsAsync in location with address {Address}", address.ToString());
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> LocationsExistsAsync(Guid[] locationIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingLocationIds = await _innerRepo.LocationsExistsAsync(locationIds, cancellationToken: cancellationToken);
            if (existingLocationIds.IsFailure)
                return existingLocationIds.Error;
        
            return existingLocationIds;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while check LocationsExistsAsync");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while check LocationsExistsAsync");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<List<Guid>, Error>> GetActiveLocationsIdsAsync(
        Guid[] locationIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activeLocationsIds = await _innerRepo.GetActiveLocationsIdsAsync(locationIds, cancellationToken: cancellationToken);
            if (activeLocationsIds.IsFailure)
                return activeLocationsIds.Error;

            return activeLocationsIds; 
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while GetActiveLocationsIdsAsync");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while GetActiveLocationsIdsAsync");
            return GeneralErrors.DatabaseError();
        }
    }
}