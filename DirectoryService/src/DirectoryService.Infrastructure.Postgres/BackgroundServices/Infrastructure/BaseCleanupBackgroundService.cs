using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public abstract class BaseCleanupBackgroundService : BackgroundService
{
    protected readonly CleanupServiceOptions Options;
    protected readonly ILogger Logger;

    protected const string RETENTION_DAYS = "retentionDays";
    protected const string BATCH_SIZE = "batchSize";
    
    protected BaseCleanupBackgroundService(
        IOptions<CleanupServiceOptions> options,
        ILogger logger)
    {
        Options = options.Value;
        Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!Options.Enabled)
        {
            Logger.LogInformation("Cleanup background service is disabled");
            return;
        }

        var cleanupInterval = TimeSpan.FromHours(Options.IntervalHours);
        using var timer = new PeriodicTimer(cleanupInterval);

        Logger.LogInformation("Service started. Will run every {Interval} hours", cleanupInterval.TotalHours);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                var cleanupResult = await CleanupAsync(cancellationToken);

                if (cleanupResult.IsFailure)
                    Logger.LogError("Cleanup failed: {Error}", cleanupResult.Error);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                Logger.LogInformation("Cleanup background service canceled.");
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while executing cleanup background service.");
            }
        }

        Logger.LogInformation("Cleanup background service stopped.");
    }

    protected abstract Task<UnitResult<Errors>> CleanupAsync(CancellationToken cancellationToken);
}