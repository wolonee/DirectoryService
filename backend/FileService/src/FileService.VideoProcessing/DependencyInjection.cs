using FileService.Core.Abstractions;
using FileService.VideoProcessing.FfmpegProcess;
using FileService.VideoProcessing.Handlers;
using FileService.VideoProcessing.Jobs;
using FileService.VideoProcessing.ProcessRunner;
using FileService.VideoProcessing.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FileService.VideoProcessing;

public static class DependencyInjection
{
    public static IServiceCollection AddVideoProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        VideoProcessingOptions options =
            configuration.GetSection(nameof(VideoProcessingOptions)).Get<VideoProcessingOptions>()
            ?? new VideoProcessingOptions();
        services.AddSingleton(options);

        // IOptions<VideoProcessingOptions> из конфига — для пайплайна/джобы (MaxRetries, RetryDelaySeconds).
        services.AddOptions<VideoProcessingOptions>()
            .Bind(configuration.GetSection(nameof(VideoProcessingOptions)));

        services.AddScoped<IProcessRunner, ProcessRunner.ProcessRunner>();
        services.AddScoped<IFfmpegProcessRunner, FfmpegProcessRunner>();

        services.AddScoped<IVideoProcessingService, VideoProcessingService>();

        services.AddScoped<IProcessingJobFactory, ProcessingJobFactory>();
        services.AddScoped<IVideoProcessingScheduler, QuartzVideoProcessingScheduler>();

        services.AddTransient<VideoProcessingJob>();

        services.AddScoped<IProcessingPipeline, ProcessingPipeline>();

        services.AddScoped<IProcessingStepHandler, InitializeStepHandler>();
        services.AddScoped<IProcessingStepHandler, ExtractMetadataStepHandler>();
        services.AddScoped<IProcessingStepHandler, GenerateHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, UploadHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, GeneratePreviewStepHandler>();
        services.AddScoped<IProcessingStepHandler, CleanupStepHandler>();

        AddQuartz(services, configuration);

        return services;
    }

    private static void AddQuartz(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("FileServiceDb")
            ?? throw new InvalidOperationException("Connection string 'FileServiceDb' is not configured.");

        services.AddQuartz(options =>
        {
            options.UsePersistentStore(persistenceOptions =>
            {
                persistenceOptions.UsePostgres(cfg => cfg.ConnectionString = connectionString);
                persistenceOptions.UseNewtonsoftJsonSerializer();
                persistenceOptions.UseProperties = false;
            });
        });

        services.AddQuartzHostedService(hostedOptions => hostedOptions.WaitForJobsToComplete = true);
    }
}
