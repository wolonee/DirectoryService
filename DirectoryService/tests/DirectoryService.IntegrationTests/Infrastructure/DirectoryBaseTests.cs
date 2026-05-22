using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    
    protected IServiceProvider Services { get; set; }
    
    public DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }
    
    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>(); 
        await action(dbContext);
    }
    
    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        return await action(dbContext);
    }
}