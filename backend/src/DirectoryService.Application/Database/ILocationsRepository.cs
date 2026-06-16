using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Database;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> AddressExistsAsync(CreateLocationAddressRequest address, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> LocationsExistsAsync(Guid[] locationIds, CancellationToken cancellationToken = default);
    
    Task<Result<IReadOnlyList<Guid>, Error>> GetActiveLocationsIdsAsync(Guid[] locationIds, CancellationToken cancellationToken = default);

    Task<Result<Location, Error>> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteDepartmentLocationsByLocationId(
        Guid locationId,
        CancellationToken cancellationToken = default);
    
    Task<UnitResult<Error>> DeleteLocationInCleanupDelete(
        Guid locationId,
        CancellationToken cancellationToken = default);
}