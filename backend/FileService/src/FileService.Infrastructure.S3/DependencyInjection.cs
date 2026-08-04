using Amazon.S3;
using FileService.Core;
using FileService.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public static class DependencyInjectionS3Extentions
{
    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(nameof(FileStorageOptions)))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<FileStorageOptions>, FileStorageOptionsValidator>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            FileStorageOptions fileStorageOptions = sp.GetRequiredService<IOptions<FileStorageOptions>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = fileStorageOptions.Endpoint, UseHttp = !fileStorageOptions.WithSsl, ForcePathStyle = true,
            };
            
            return new AmazonS3Client(fileStorageOptions.AccessKey, fileStorageOptions.SecretKey, config);
        });

        services.AddScoped<IS3Provider, S3Provider>();
        services.AddScoped<IChunkSizeCalculator, ChunkSizeCalculator>();
        
        services
            .AddHealthChecks()
            .AddCheck<S3HealthCheck>(
                "object-storage",
                timeout: TimeSpan.FromSeconds(5));
        
        services.AddHostedService<S3BucketInitializationService>();

        return services;
    }
}
