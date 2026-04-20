using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> AddressExistsAsync(CreateLocationAddressRequest address, CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> LocationsExistsAsync(Guid[] locationIds, CancellationToken cancellationToken = default);

}