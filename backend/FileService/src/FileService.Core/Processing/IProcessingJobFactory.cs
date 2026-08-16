using FileService.Domain.S3Entities.Assets;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

public interface IVideoProcessingJobFactory
{
    bool CanProcess(MediaAsset mediaAsset);
    IJobDetail CreateJob(MediaAsset mediaAsset);
    ITrigger CreateTrigger(MediaAsset mediaAsset);
}