using FileService.Domain.S3Entities.Assets;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

public interface IProcessingJobFactory
{
    bool CanProcess(MediaAsset mediaAsset);
    
    IJobDetail CreateJob(MediaAsset mediaAsset);
    
    ITrigger CreateTrigger(MediaAsset mediaAsset);
    
    // ITrigger CreateRetryTrigger(MediaAsset mediaAsset, int retryCount, DateTime startAtUtc);
}