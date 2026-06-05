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

namespace DirectoryService.Infrastructure.BackgroundServices;

public class HardDeleteDepartmentsService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HardDeleteDepartmentsService> _logger;

    private const string TIME = "time";
    
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1);

    public HardDeleteDepartmentsService(
        IServiceScopeFactory scopeFactory,
        ILogger<HardDeleteDepartmentsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_cleanupInterval);

        _logger.LogInformation("Service started. Will run every {Interval} hours", _cleanupInterval.TotalHours);
        _logger.LogInformation("Service started. Will run every {Interval} hours", _cleanupInterval.TotalHours);
        _logger.LogInformation("Service started. Will run every {Interval} hours", _cleanupInterval.TotalHours);
        _logger.LogInformation("Service started. Will run every {Interval} hours", _cleanupInterval.TotalHours);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var cleanupResult = await CleanupAsync(stoppingToken);
                if (cleanupResult.IsFailure)
                {
                    _logger.LogError($"Cleanup failed: {cleanupResult.Error}");
                    throw new Exception("Cleanup failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
                throw;
            }
        }
    }

    private async Task<UnitResult<Errors>> CleanupAsync(CancellationToken cancellationToken)
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

        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        parameters.Add(TIME, cutoffDate, DbType.DateTime);
        
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
            WHERE d.is_deleted AND d.deleted_at < @{TIME}
            ORDER BY d.depth DESC;
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
                _logger.LogError($"Failed to update parent in cleanup departments: {updateParentResult.Error.ToErrors()}");
                transactionScope.Rollback();
                return updateParentResult.Error.ToErrors();
            }

            var deleteDepartmentsResult = await departmentsRepository.DeleteDepartmentInCleanupDelete(department.Id, cancellationToken);
            if (deleteDepartmentsResult.IsFailure)
            {
                _logger.LogError($"Failed to delete department in cleanup departments: {updateParentResult.Error.ToErrors()}");
                transactionScope.Rollback();
                return deleteDepartmentsResult.Error.ToErrors();
            }
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogError("Error during cleanup Commit");
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }

        return UnitResult.Success<Errors>();
    }
}