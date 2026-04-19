using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public async Task<Result<List<string>, Error>> GetActiveFullNames(
        string direction, 
        string speciality,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var getDirection = await _dbContext.Positions
                .Where(x => x.IsActive)
                .Where(d => d.Name.Direction == direction)
                .Where(s => s.Name.Speciality == speciality)
                .Select(f => PositionName.GetFullName(f.Name.Speciality, f.Name.Direction))
                .ToListAsync(cancellationToken: cancellationToken);

            if (!getDirection.Any())
                return PositionErrors.NotFoundNames();

            return getDirection;
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