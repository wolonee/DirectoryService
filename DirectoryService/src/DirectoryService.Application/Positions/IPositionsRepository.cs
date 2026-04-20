using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<List<string>, Error>> GetActiveFullNames(string direction, string speciality, CancellationToken cancellationToken = default);
    
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);

}