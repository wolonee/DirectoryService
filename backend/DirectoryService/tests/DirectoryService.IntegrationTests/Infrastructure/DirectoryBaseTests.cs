using System.Net.Http.Json;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    
    protected IServiceProvider Services { get; set; }
    
    protected HttpClient AppHttpClient { get; set; }
    
    protected HttpClient HttpClient { get; set; }
    
    protected static void AssertErrorType(Errors errors, ErrorType errorType)
    {
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Type == errorType);
    }

    public DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
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
            var cancellationToken = new CancellationTokenSource().Token;
            
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
    
    protected async Task<Department> CreateParentDepartment(bool? active = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var cancellationToken = new CancellationTokenSource().Token;
            
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

            return department;
        });
    }
    
    protected async Task<Department> CreateChildDepartment(Department parent, string? difference = "special",  bool? active = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var cancellationToken = new CancellationTokenSource().Token;
            
            difference = difference?.ToLower();
            var locationId = await CreateLocation($"{difference}Street", "Moscow", "Russia", $"{difference}Office");
            
            var departmentId = Guid.NewGuid();
            var departmentLocations = DepartmentLocation.Create(departmentId, locationId).Value;

            var department = Department.CreateChild(
                departmentId,
                DepartmentName.Create($"{difference}DepartmentName").Value,
                DepartmentIdentifier.Create($"{difference}_child").Value,
                parent,
                [departmentLocations]).Value;
            
            if (active == false)
                department.Activate(false);
            
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(cancellationToken);

            return department;
        });
    }

    protected static CreateLocationRequest BuildCreateLocationRequest(
        string name = "TestOffice",
        string street = "TestStreet",
        string city = "Moscow",
        string country = "Russia",
        string timezone = "Europe/Moscow")
    {
        return new CreateLocationRequest(
            new CreateLocationAddressRequest(country, city, street),
            name,
            timezone);
    }

    protected async Task<Guid> CreateLocationViaApi(
        CreateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await AppHttpClient.PostAsJsonAsync("/api/location", request, cancellationToken);
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: cancellationToken);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    protected async Task AttachLocationToDepartment(Guid departmentId, Guid locationId, CancellationToken cancellationToken = default)
    {
        await ExecuteInDb(async dbContext =>
        {
            var link = DepartmentLocation.Create(departmentId, locationId).Value;
            dbContext.DepartmentLocations.Add(link);
            await dbContext.SaveChangesAsync(cancellationToken);
        });
    }

    protected async Task<Guid> CreatePositionViaApi(
        Guid departmentId,
        string speciality,
        string direction,
        CancellationToken cancellationToken = default)
    {
        var request = new CreatePositionRequest(
            new CreatePositionNameRequest(speciality, direction),
            null,
            [departmentId]);

        var response = await AppHttpClient.PostAsJsonAsync("/api/positions", request, cancellationToken);
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: cancellationToken);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    protected async Task<Guid> CreatePositionInDb(
        string speciality,
        string direction,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var position = Position.Create(
                Guid.NewGuid(),
                PositionName.Create(speciality, direction).Value,
                null).Value;

            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync(cancellationToken);
            return position.Id;
        });
    }
}