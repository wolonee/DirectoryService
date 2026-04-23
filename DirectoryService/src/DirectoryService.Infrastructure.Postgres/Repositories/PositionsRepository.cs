using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public PositionsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        _dbContext.Positions.Add(position);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return position.Id;
    }
    
    public async Task<Result<List<string>, Error>> GetActiveFullNames(
        string direction, 
        string speciality,
        CancellationToken cancellationToken = default)
    {
        var getFullName = await _dbContext.Positions
            .Where(x => x.IsActive)
            .Where(d => d.Name.Direction == direction)
            .Where(s => s.Name.Speciality == speciality)
            .Select(f => PositionName.GetFullName(f.Name.Speciality, f.Name.Direction))
            .ToListAsync(cancellationToken: cancellationToken);

        return getFullName;
    }
}