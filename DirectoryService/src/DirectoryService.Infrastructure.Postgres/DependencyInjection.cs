using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
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
        
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        
        services.AddScoped<IPositionsRepository, PositionsRepository>();
        
        return services;
    }
}