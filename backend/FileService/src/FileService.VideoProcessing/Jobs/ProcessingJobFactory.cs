using FileService.Domain.S3Entities.Assets;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

public class ProcessingJobFactory : IProcessingJobFactory
{
    private const string JOB_GROUP = "video-processing";

    public bool CanProcess(MediaAsset mediaAsset)
    {
        return mediaAsset is VideoAsset;
    }

    public IJobDetail CreateJob(MediaAsset mediaAsset)
    {
        return JobBuilder.Create<VideoProcessingJob>()
            .WithIdentity($"video-processing-{mediaAsset.Id}", JOB_GROUP)
            .UsingJobData(VideoProcessingJob.VideoAssetIdKey.Name, mediaAsset.Id.ToString())
            .StoreDurably(true)
            .RequestRecovery(true)
            .Build();
    }

    public ITrigger CreateTrigger(MediaAsset mediaAsset)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"video-processing-trigger-{mediaAsset.Id}", JOB_GROUP)
            .ForJob($"video-processing-{mediaAsset.Id}", JOB_GROUP)
            .StartNow()
            .Build();
    }

    public ITrigger CreateRetryTrigger(Guid mediaAssetId, int retryCount, DateTime startAtUtc)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"video-processing-retry-trigger-{mediaAssetId}-{retryCount}", JOB_GROUP)
            .ForJob($"video-processing-{mediaAssetId}", JOB_GROUP)
            .StartAt(startAtUtc)
            .Build();
    }
}