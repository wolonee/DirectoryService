using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Database;

public interface IPositionsRepository
{
    Task<Result<List<string>, Error>> GetActiveFullNames(string direction, string speciality, CancellationToken cancellationToken = default);
    
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);

    Task<Result<Position, Error>> GetByIdAsync(Guid positionId, CancellationToken cancellationToken = default);

    Task<Result<bool, Error>> ActiveFullNameExistsAsync(
        string direction,
        string speciality,
        Guid? excludePositionId = null,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeletePositionInCleanupDelete(
        Guid positionId,
        CancellationToken cancellationToken = default);
}