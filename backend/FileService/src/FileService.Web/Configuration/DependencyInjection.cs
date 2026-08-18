using FileService.Core;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using FileService.Core.Abstractions;
using FileService.VideoProcessing;
using FileService.Web.Auth;
using FileService.Web.EndpointsExtensions;
using Serilog;
using Serilog.Exceptions;

namespace FileService.Web.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDependencies(configuration);
        services.AddCoreDependencies(configuration);
        services.AddS3(configuration);
        services.AddVideoProcessing(configuration);
        services.AddQuartzServices(configuration);
        services.AddWebDependencies();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddSerilogLogging(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Realtime-прогресс через SignalR: сам фреймворк + фоновый мост очередь→хаб.
        services.AddSignalR();
        services.AddHostedService<Progress.ProgressBroadcastConsumer>();

        // Чтение initial-снапшота из БД для отправки только что подписавшемуся клиенту.
        services.AddScoped<Progress.InitialProcess>();

        return services;
    }
    
    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "FileService"));

        return services;
    }
}
