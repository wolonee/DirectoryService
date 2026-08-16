using DirectoryService.Application.Abstractions;
using FileService.Core.Features;
using FileService.Core.Features.MultipartUpload;
using FileService.Core.Features.Simple;
using FileService.Core.Options;
using FileService.Core.Options.CacheOptions;
using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace FileService.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpoints(typeof(InitiateUploadEndpoint).Assembly);
        services.AddScoped<IMediaAssetFactory, MediaAssetFactory>();
        
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        var assembly = typeof(StartMultipartUploadHandler).Assembly;
        
        services.AddHandlers(assembly);

        services
            .AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(nameof(CacheOptions)))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<CacheOptions>, CacheOptionsValidator>();

        var cacheOptions = configuration.GetSection(nameof(CacheOptions)).Get<CacheOptions>()
                           ?? new CacheOptions();

        services.AddStackExchangeRedisCache(setup =>
        {
            setup.Configuration = cacheOptions.RedisEndpoint;
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = cacheOptions.PresignedUrlTtl,
                LocalCacheExpiration = cacheOptions.LocalCacheTtl,
            };
        });
        
        return services;
    }
    
    public static IServiceCollection AddQuartzServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddQuartz(options =>
        {
            options.UsePersistentStore(persistenceOptions =>
            {
                persistenceOptions.UsePostgres(cfg =>
                {
                    cfg.ConnectionString = configuration.GetConnectionString("FileServiceDb")!;
                });

                persistenceOptions.UseNewtonsoftJsonSerializer();
                persistenceOptions.UseProperties = false;
            });
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
