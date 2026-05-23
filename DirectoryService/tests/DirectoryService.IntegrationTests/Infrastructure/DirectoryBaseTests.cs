using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    protected IServiceProvider Services { get; set; }
    
    protected CancellationToken cancellationToken = CancellationToken.None;

    
    public DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() =>
        await _resetDatabase();
    
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
    
    protected async Task<Guid> CreateLocation(string street, string city, string country, string name, bool? active = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                LocationAddress.Create(street, city, country).Value,
                LocationName.Create(name).Value,
                LocationTimeZone.Create("Europe/Moscow").Value).Value;
            
            if (active == false)
                location.Activate(false);
            
            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync(cancellationToken);

            return location.Id;
        });
    }
    
    protected async Task<Guid> CreateDepartment(bool? active = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var locationId = await CreateLocation("specialStreet", "Moscow", "Russia", "specialOffice");
            
            var departmentId = Guid.NewGuid();
            var departmentLocations = DepartmentLocation.Create(departmentId, locationId).Value;

            var department = Department.CreateParent(
                DepartmentName.Create("ParentDepartment").Value,
                DepartmentIdentifier.Create("parent").Value,
                [departmentLocations],
                departmentId).Value;


            if (active == false)
                department.Activate(false);


            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(cancellationToken);

            return department.Id;
        });
    }
}