using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
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
    
    public LocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id;
    }
    
    public async Task<Result<bool, Error>> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.AnyAsync(x => x.Name.Value == name, cancellationToken: cancellationToken);
    }

    public async Task<Result<bool, Error>> AddressExistsAsync(CreateLocationAddressRequest address, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.AnyAsync(
            x => x.Address.Country == address.Country && 
                 x.Address.Street == address.Street && 
                 x.Address.City == address.City, 
            cancellationToken);
    }

    public async Task<Result<bool, Error>> LocationsExistsAsync(Guid[] locationIds, CancellationToken cancellationToken = default)
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
}