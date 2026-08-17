using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities.Assets;
using FileService.VideoProcessing.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FileService.VideoProcessing.Scheduling;

/// <summary>
/// Quartz-реализация порта <see cref="IVideoProcessingScheduler"/>.
/// Инкапсулирует выбор фабрики и работу с планировщиком, чтобы Core о Quartz не знал.
/// </summary>
public sealed class QuartzVideoProcessingScheduler : IVideoProcessingScheduler
{
    private readonly IEnumerable<IProcessingJobFactory> _jobFactories;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzVideoProcessingScheduler> _logger;

    public QuartzVideoProcessingScheduler(
        IEnumerable<IProcessingJobFactory> jobFactories,
        ISchedulerFactory schedulerFactory,
        ILogger<QuartzVideoProcessingScheduler> logger)
    {
        _jobFactories = jobFactories;
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> ScheduleAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        IProcessingJobFactory? factory = _jobFactories.FirstOrDefault(f => f.CanProcess(mediaAsset));
        if (factory is null)
        {
            _logger.LogError("No processing job factory found for MediaAssetId: {MediaAssetId}", mediaAsset.Id);
            return Error.Failure("processing.scheduler.no-factory", "No processing job factory found");
        }

        IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        IJobDetail job = factory.CreateJob(mediaAsset);
        ITrigger trigger = factory.CreateTrigger(mediaAsset);

        await scheduler.ScheduleJob(job, trigger, cancellationToken);

        _logger.LogInformation("Scheduled processing job for MediaAssetId: {MediaAssetId}", mediaAsset.Id);

        return UnitResult.Success<Error>();
    }
}
