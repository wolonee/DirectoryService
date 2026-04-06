using DirectoryService.Application;
using DirectoryService.Infrastructure;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services)
    {
        services.AddApplication();
        services.AddInfrastructure();
        services.AddWebDependencies();
        
        return services;
    }
    
    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        
        return services;
    }
}