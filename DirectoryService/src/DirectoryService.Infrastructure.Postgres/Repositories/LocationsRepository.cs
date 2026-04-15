using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(DirectoryServiceDbContext dbContext, ILogger<LocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        _dbContext.Locations.Add(location);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return location.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return LocationErrors.NameConflict(location.Name.Value);
            }

            _logger.LogError(ex, "Database update error while creating location with name {Name}", location.Name.Value);
            return LocationErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating location with name {Name}", location.Name.Value);
            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating location with name {Name}", location.Name.Value);
            return LocationErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Locations.AnyAsync(x => x.Name.Value == name);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while checking NameExists with name {Name}", name);
            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating NameExists with name {Name}", name);
            return LocationErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> AddressExistsAsync(CreateLocationAddressRequest address, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Locations.AnyAsync(
                x => x.Address.Street == address.Street && 
                     x.Address.City == address.City && 
                     x.Address.Country == address.Country, 
                cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while checking AddressExists with address {Address}", address.ToString());
            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating AddressExists with address {Address}", address.ToString());
            return LocationErrors.DatabaseError();
        }
    }
}