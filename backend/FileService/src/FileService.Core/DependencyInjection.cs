using FileService.Web.EndpointsExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IS3Provider>();
        
        return services;
    }
}