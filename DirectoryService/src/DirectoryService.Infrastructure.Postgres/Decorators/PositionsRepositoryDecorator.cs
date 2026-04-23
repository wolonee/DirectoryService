using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Decorators;

public class PositionsRepositoryDecorator : IPositionsRepository
{
    private readonly PositionsRepository _innerRepo;
    private readonly ILogger<PositionsRepositoryDecorator> _logger;

    public PositionsRepositoryDecorator(
        PositionsRepository positionsRepository,
        ILogger<PositionsRepositoryDecorator> logger)
    {
        _innerRepo = positionsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        try
        {
            var addPositionResult = await _innerRepo.AddAsync(position, cancellationToken);
            if (addPositionResult.IsFailure)
                return addPositionResult.Error;

            return addPositionResult;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            _logger.LogError(ex, "Database update error while AddAsync position with id {Id}", position.Id);
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while AddAsync position with id {Id}", position.Id);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while AddAsync position with id {Id}", position.Id);
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
            var getFullName = await _innerRepo.GetActiveFullNames(direction, speciality, cancellationToken);
            if (getFullName.IsFailure)
                return getFullName.Error;
            
            return getFullName;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while GetActiveFullNames position");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while GetActiveFullNames position");
            return GeneralErrors.DatabaseError();
        }
    }
}