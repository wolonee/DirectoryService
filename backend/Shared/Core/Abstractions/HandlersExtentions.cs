using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application.Abstractions;

public static class HandlersExtentions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
        
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(IQueryHandler<,>), typeof(IQueryHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
        
        return services;
    }
}