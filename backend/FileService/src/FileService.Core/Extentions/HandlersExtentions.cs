using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace FileService.Core.Extentions;

// public static class HandlersExtentions
// {
//     public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
//     {
//         services.Scan((Action<ITypeSourceSelector>) (scan => scan.FromAssemblies(assembly).AddClasses((Action<IImplementationTypeFilter>) (classes => classes.AssignableToAny(typeof (ICommandHandler<,>), typeof (ICommandHandler<>)))).AsSelfWithInterfaces().WithScopedLifetime()));
//         services.Scan((Action<ITypeSourceSelector>) (scan => scan.FromAssemblies(assembly).AddClasses((Action<IImplementationTypeFilter>) (classes => classes.AssignableToAny(typeof (IQueryHandler<,>), typeof (IQueryHandler<>)))).AsSelfWithInterfaces().WithScopedLifetime()));
//         return services;
//     }
// }
