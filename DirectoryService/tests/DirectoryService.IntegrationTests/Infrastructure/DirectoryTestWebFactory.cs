using System.Data.Common;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Presentation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("directory_service_db_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        // .WithPortBinding(62210, 5432)
        .Build();
    
    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync(); 
        
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        Console.WriteLine(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        await InitializeRespawner();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        
        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
    }
    
    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);    
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DirectoryServiceDbContext>();
            services.RemoveAll<IReadDbContext>();

            services.AddScoped<DirectoryServiceDbContext>(_ =>
                new DirectoryServiceDbContext(_dbContainer.GetConnectionString()));
            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<DirectoryServiceDbContext>());
        });
    }

    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres, 
                SchemasToInclude = ["public"],
            });
    }
}