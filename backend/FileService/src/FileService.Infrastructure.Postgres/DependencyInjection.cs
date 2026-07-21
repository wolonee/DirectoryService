using FileService.Infrastructure.Postgres.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgresDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("FileServiceDb")
            ?? throw new InvalidOperationException("Connection string 'FileServiceDb' is not configured.");

        services.AddScoped<FileServiceDbContext>(_ => new FileServiceDbContext(connectionString));

        return services;
    }
}
