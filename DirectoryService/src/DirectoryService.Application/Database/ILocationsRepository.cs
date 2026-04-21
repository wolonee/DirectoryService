using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;

namespace DirectoryService.Application.Database;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> AddressExistsAsync(CreateLocationAddressRequest address, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> LocationsExistsAsync(Guid[] locationIds, CancellationToken cancellationToken = default);
    
    Task<Result<List<Guid>, Error>> GetActiveLocationsIdsAsync(Guid[] locationIds, CancellationToken cancellationToken = default);
    
    Task<UnitResult<Error>> DeleteLocationsByDepartmentId(Guid departmentId, CancellationToken cancellationToken = default);


}