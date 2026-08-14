using FileService.Core.Abstractions;
using FileService.Infrastructure.Postgres.Database;
using FileService.Infrastructure.Postgres.Repositories;
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
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<FileServiceDbContext>());
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IVideoAssetRepository, VideoAssetRepository>();
        services.AddScoped<IVideoProcessingRepository, VideoProcessingRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}
