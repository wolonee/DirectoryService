using DirectoryService.Application;
using DirectoryService.Infrastructure;
using FileService.Contracts.HttpCommunication;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors();

        services.AddApplication(configuration);
        services.AddInfrastructure();
        services.AddFileServiceHttpCommunication(configuration);
        services.AddWebDependencies();

        services.AddSerilogLogging(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        
        return services;
    }
    
    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService"));

        return services;
    }
}
