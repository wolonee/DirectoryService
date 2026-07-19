using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Infrastructure.S3;

public static class DependencyInjection
{
    public static IServiceCollection AddS3Dependencies(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}