using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Decorators;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ITransactionManager, TransactionManager>();
        
        services.AddScoped<DepartmentsRepository>();

        services.AddScoped<IDepartmentsRepository>(sp =>
        {
            var repo = sp.GetRequiredService<DepartmentsRepository>();
            var logger = sp.GetRequiredService<ILogger<DepartmentsRepositoryDecorator>>();
            return new DepartmentsRepositoryDecorator(repo, logger);
        });
        
        services.AddScoped<LocationsRepository>();

        services.AddScoped<ILocationsRepository>(sp =>
        {
            var repo = sp.GetRequiredService<LocationsRepository>();
            var logger = sp.GetRequiredService<ILogger<LocationsRepositoryDecorator>>();
            return new LocationsRepositoryDecorator(repo, logger);
        });

        services.AddScoped<PositionsRepository>();

        services.AddScoped<IPositionsRepository>(sp =>
        {
            var repo = sp.GetRequiredService<PositionsRepository>();
            var logger = sp.GetRequiredService<ILogger<PositionsRepositoryDecorator>>();
            return new PositionsRepositoryDecorator(repo, logger);
        });
        
        return services;
    }
}