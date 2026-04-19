using CSharpFunctionalExtensions;
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
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(DirectoryServiceDbContext dbContext, ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        _dbContext.Positions.Add(position);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return position.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            _logger.LogError(ex, "Database update error while creating position with id {Id}", position.Id);
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating position with id {Id}", position.Id);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating position with id {Id}", position.Id);
            return GeneralErrors.DatabaseError();
        }
    }
    
    public async Task<Result<List<string>, Error>> GetActiveFullNames(
        string direction, 
        string speciality,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var getFullName = await _dbContext.Positions
                .Where(x => x.IsActive)
                .Where(d => d.Name.Direction == direction)
                .Where(s => s.Name.Speciality == speciality)
                .Select(f => PositionName.GetFullName(f.Name.Speciality, f.Name.Direction))
                .ToListAsync(cancellationToken: cancellationToken);

            return getFullName;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while getting position names");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting position names");
            return GeneralErrors.DatabaseError();
        }
    }
}