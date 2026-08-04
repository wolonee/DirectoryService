using DirectoryService.Application.Abstractions;
using FileService.Core.Features;
using FileService.Core.Features.SimpleUpload;
using FileService.Core.Options;
using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
}
