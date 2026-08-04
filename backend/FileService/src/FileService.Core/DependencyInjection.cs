using DirectoryService.Application.Abstractions;
using FileService.Core.Features;
using FileService.Core.Features.SimpleUpload;
using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddStackExchangeRedisCache(setup =>
        {
            setup.Configuration = "localhost:6379";
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions()
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5), Expiration = TimeSpan.FromMinutes(10),
            };
        });
        
        return services;
    }
}
