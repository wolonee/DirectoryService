using Microsoft.Extensions.Logging;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

// [DisallowConcurrentExecution]
// public class VideoProcessingJob : IJob
// {
//     public static readonly JobKey VideoAssetIdKey = new("VideoAssetId");
//
//     private readonly ILogger<VideoProcessingJob> _logger;
//     private readonly IVideoProcessingService _videoProcessingService;
//
//     public VideoProcessingJob(
//         ILogger<VideoProcessingJob> logger,
//         IVideoProcessingService videoProcessingService)
//     {
//         _logger = logger;
//         _videoProcessingService = videoProcessingService;
//     }
//
//     public async Task Execute(IJobExecutionContext context)
//     {
//         _logger.LogInformation(
//             "Starting video processing job for VideoAssetId: {VideoAssetId}");
//
//         await _videoProcessingService.ProcessVideoAsync(context.CancellationToken);
//     }
// }