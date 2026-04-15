using DirectoryService.Application.Locations;
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
    
    public async Task<Guid, Error> AddAsync(Location location, CancellationToken cancellationToken = default)
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
}