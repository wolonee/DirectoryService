using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Database;

public interface IPositionsRepository
{
    Task<Result<List<string>, Error>> GetActiveFullNames(string direction, string speciality, CancellationToken cancellationToken = default);
    
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);

}