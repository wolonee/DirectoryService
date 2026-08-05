using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Commands.CreateLocation;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        var assembly = typeof(CreateLocationHandler).Assembly;
        
        services.AddHandlers(assembly);

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
                Expiration = cacheOptions.Expiration, 
                LocalCacheExpiration = cacheOptions.LocalCacheExpiration,
            };
        });
        
        return services;
    }
}