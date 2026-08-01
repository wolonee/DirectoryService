using Amazon.S3;
using FileService.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public static class DependencyInjectionS3Extentions
{
    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<S3Options>()
            .Bind(configuration.GetSection(nameof(S3Options)))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<S3Options>, S3OptionsValidator>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            S3Options s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = s3Options.Endpoint, UseHttp = !s3Options.WithSsl, ForcePathStyle = true,
            };
            
            return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
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
