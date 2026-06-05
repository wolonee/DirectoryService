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

public class CleanupLocationsService : BaseCleanupBackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CleanupLocationsService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<CleanupServiceOptions> options,
        ILogger<CleanupLocationsService> logger)
        : base(options, logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }


    protected override async Task<UnitResult<Errors>> CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        var locationsRepository = scope.ServiceProvider.GetRequiredService<ILocationsRepository>();

        var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        
        var cutoffDate = DateTime.UtcNow.AddDays(-Options.RetentionDays);
        parameters.Add(RETENTION_DAYS, cutoffDate, DbType.DateTime);
        parameters.Add(BATCH_SIZE, Options.BatchSize, DbType.Int32);

        var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;


        var result = connection.QueryAsync<Guid>(
            $"""
            SELECT l.id
            FROM locations l
            WHERE d.is_deleted
              AND d.deleted_at < @{RETENTION_DAYS}
            LIMIT @{BATCH_SIZE}
            """,
            param: parameters);

        if (result.IsFaulted)
        {
            Logger.LogError("Error cleaning up locations");
            return Error.Conflict("conflict.error", "Error cleaning up locations").ToErrors();
        }
            
        foreach (var id in result.Result)
        {
            var boolResult = await locationsRepository.DeleteLocationInCleanupDelete(id, cancellationToken);
            if (boolResult.IsFailure)
            {
                Logger.LogError($"Failed to delete location with id {id}.");
                return boolResult.Error.ToErrors();
            }
        }

        var saveChangesAsyncResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesAsyncResult.IsFailure)
            return saveChangesAsyncResult.Error.ToErrors();

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();
        
        return UnitResult.Success<Errors>();
    }
}