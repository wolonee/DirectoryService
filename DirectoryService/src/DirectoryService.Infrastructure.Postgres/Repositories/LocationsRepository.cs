using System.Linq.Expressions;
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
    
    public async Task<Result<List<Location>, Error>> GetAsync(
        Expression<Func<Location, bool>>? predicate = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Locations.AsQueryable();
        
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }
        
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
        
            var locations = await query.ToListAsync(cancellationToken);
            return locations;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while getting locations");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting locations");
            return GeneralErrors.DatabaseError();
        }
    }
    
    public async Task<Result<Location, Error>> GetFirstAsync(
        Expression<Func<Location, bool>>? predicate = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Locations.AsQueryable();
        
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }
        
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
        
            var location = await query.FirstOrDefaultAsync(cancellationToken);
        
            if (location is null)
            {
                return GeneralErrors.NotFound();
            }
        
            return location;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while getting locations");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting locations");
            return GeneralErrors.DatabaseError();
        }
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
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating location with name {Name}", location.Name.Value);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating location with name {Name}", location.Name.Value);
            return GeneralErrors.DatabaseError();
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
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating NameExists with name {Name}", name);
            return GeneralErrors.DatabaseError();
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
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating AddressExists with address {Address}", address.ToString());
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> LocationsExistsAsync(Guid[] locationIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingLocationIds = await _dbContext.Locations
                .Where(x => locationIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (existingLocationIds.Count != locationIds.Length)
            {
                return false;
            }
        
            return true;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while checking locations existence");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking locations existence");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<List<Guid>, Error>> GetActiveLocationsIdsAsync(
        Guid[] locationIds,
        CancellationToken cancellationToken = default)
    {
        var activeLocationsIds = await _dbContext.Locations
            .Where(x => locationIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        return activeLocationsIds;
    }

    public async Task<UnitResult<Error>> DeleteLocationsByDepartmentId(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Locations
            .Where(x => x.Id == departmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}