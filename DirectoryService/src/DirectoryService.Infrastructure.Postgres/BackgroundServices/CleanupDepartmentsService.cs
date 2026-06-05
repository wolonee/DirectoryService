using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupDepartmentsService : BaseCleanupBackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CleanupDepartmentsService(
        IOptions<CleanupServiceOptions> options,
        ILogger<CleanupDepartmentsService> logger,
        IServiceScopeFactory scopeFactory)
        : base(options, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<UnitResult<Errors>> CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var departmentsRepository = scope.ServiceProvider.GetRequiredService<IDepartmentsRepository>();
            
        var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        
        var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var cutoffDate = DateTime.UtcNow.AddDays(-Options.RetentionDays);
        parameters.Add(RETENTION_DAYS, cutoffDate, DbType.DateTime);
        parameters.Add(BATCH_SIZE, Options.BatchSize, DbType.Int32);
        
        var expiredDepartments = await connection.QueryAsync<CleanupDepartmentDto>(
            $"""
            SELECT d.id,
                   d.parent_id,
                   d.identifier,
                   d.path,
                   d.depth,
                   parent.path AS parent_path
            FROM department d
            JOIN department parent ON parent.id = d.parent_id
            WHERE d.is_deleted AND d.deleted_at < @{RETENTION_DAYS}
            ORDER BY d.depth DESC
            LIMIT @{BATCH_SIZE};
            """,
            param: parameters);

        foreach (var department in expiredDepartments)
        {
            var updateParentResult = await departmentsRepository.UpdateParentInCleanupDelete(
                department.Path,
                department.ParentPath,
                department.Id,
                department.ParentId,
                cancellationToken);
            if (updateParentResult.IsFailure)
            {
                Logger.LogError($"Failed to update parent in cleanup departments: {updateParentResult.Error.ToErrors()}");
                transactionScope.Rollback();
                return updateParentResult.Error.ToErrors();
            }

            var deleteDepartmentsResult = await departmentsRepository.DeleteDepartmentInCleanupDelete(department.Id, cancellationToken);
            if (deleteDepartmentsResult.IsFailure)
            {
                Logger.LogError($"Failed to delete department in cleanup departments: {updateParentResult.Error.ToErrors()}");
                transactionScope.Rollback();
                return deleteDepartmentsResult.Error.ToErrors();
            }
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            Logger.LogError("Error during cleanup Commit");
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }

        return UnitResult.Success<Errors>();
    }
}