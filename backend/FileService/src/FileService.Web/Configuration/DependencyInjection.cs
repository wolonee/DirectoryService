using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using FileService.Web.EndpointsExtensions;
using Serilog;
using Serilog.Exceptions;

namespace FileService.Web.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDependencies(configuration);
        services.AddS3(configuration);
        
        services.AddWebDependencies();

        services.AddSerilogLogging(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        return services;
    }
    
    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "LessonService"));

        return services;
    }
}
