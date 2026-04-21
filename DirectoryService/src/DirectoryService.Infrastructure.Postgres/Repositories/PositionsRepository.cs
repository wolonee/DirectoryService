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
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(DirectoryServiceDbContext dbContext, ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<List<Position>, Error>> GetAsync(
        Expression<Func<Position, bool>>? predicate = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Positions.AsQueryable();
        
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }
        
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
        
            var positions = await query.ToListAsync(cancellationToken);
            return positions;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while getting positions");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting positions");
            return GeneralErrors.DatabaseError();
        }
    }
    
    public async Task<Result<Position, Error>> GetFirstAsync(
        Expression<Func<Position, bool>>? predicate = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Positions.AsQueryable();
        
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }
        
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
        
            var position = await query.FirstOrDefaultAsync(cancellationToken);
        
            if (position is null)
            {
                return GeneralErrors.NotFound();
            }
        
            return position;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while getting positions");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting positions");
            return GeneralErrors.DatabaseError();
        }
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