using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupPositionsService : BaseCleanupBackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CleanupPositionsService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<CleanupServiceOptions> options,
        ILogger logger) 
        : base(options, logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task<UnitResult<Errors>> CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        var positionsRepository = scope.ServiceProvider.GetRequiredService<IPositionsRepository>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();

        var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            Logger.LogError("Transaction failed");
            return transactionScopeResult.Error.ToErrors();
        }
        
        using var transactionScope = transactionScopeResult.Value;
        
        var connect = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        
        var cutoffDate = DateTime.UtcNow.AddDays(-Options.RetentionDays);
        parameters.Add(RETENTION_DAYS, cutoffDate, DbType.DateTime);
        parameters.Add(BATCH_SIZE, Options.BatchSize, DbType.Int32);

        var result = await connect.QueryAsync<Guid>(
            $"""
             SELECT p.id
             FROM position p
             WHERE p.is_deleted
                 AND p.deleted_at < @{RETENTION_DAYS}
             LIMIT @{BATCH_SIZE}
             """,
            param: parameters);
        
        foreach (var id in result)
        {
            var positionDeleteResult = await positionsRepository.DeletePositionInCleanupDelete(id, cancellationToken);
            if (positionDeleteResult.IsFailure)
            {
                Logger.LogError($"Failed to delete position with id {id}.");
                transactionScope.Rollback();
                return positionDeleteResult.Error.ToErrors();
            }
        }

        var saveChangesAsyncResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesAsyncResult.IsFailure)
        {
            Logger.LogError("Failed to save changes.");
            transactionScope.Rollback();
            return saveChangesAsyncResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            Logger.LogError("Commit failed");
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }
        
        return UnitResult.Success<Errors>();
    }
}