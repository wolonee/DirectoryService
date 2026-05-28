using System.Net.Http.Json;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateLocationsTests : DirectoryBaseTests
{
    public UpdateLocationsTests(DirectoryTestWebFactory factory)
        : base(factory) { }
    
    [Fact]
    public async Task Update_many_location_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var department = await CreateParentDepartment();
        var departmentId = department.Id;

        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");
        var locationId2 = await CreateLocation("dddd", "Moscow", "Russia", "Office_2");
        var locationId3 = await CreateLocation("aaaa", "Moscow", "Russia", "Office_3");

        var locationsIds = new[] { locationId1, locationId2, locationId3 };

        var request = new UpdateLocationsRequest(locationsIds);
        
        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentId}/locations", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        
        await ExecuteInDb(async dbContext =>
        {
            var departmentLocations = await dbContext.DepartmentLocations
                .Where(dl => dl.DepartmentId == departmentId)
                .Select(ld => ld.LocationId)
                .ToListAsync(cancellationToken);
            
            Assert.Equal(3, departmentLocations.Count);
            Assert.Contains(locationId1, departmentLocations);
            Assert.Contains(locationId2, departmentLocations);
            Assert.Contains(locationId3, departmentLocations);
        });
    }
    
    [Fact]
    public async Task Update_one_location_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var department = await CreateParentDepartment();
        var departmentId = department.Id;        
        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var locationsIds = new[] { locationId1 };
        
        var request = new UpdateLocationsRequest(locationsIds);
        
        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentId}/locations", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        
        await ExecuteInDb(async dbContext =>
        {
            var departmentLocations = await dbContext.DepartmentLocations
                .Where(dl => dl.DepartmentId == departmentId)
                .Select(ld => ld.LocationId)
                .ToListAsync(cancellationToken);
            
            Assert.Single(departmentLocations);
            Assert.Contains(locationId1, departmentLocations);
        });
    }
    
    [Fact]
    public async Task Department_not_found_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var departmentId = Guid.NewGuid();
        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var locationsIds = new[] { locationId1 };
        
        var request = new UpdateLocationsRequest(locationsIds);
        
        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentId}/locations", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
        
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken: cancellationToken);
            
            Assert.Null(department);
        });
    }
    
    [Fact]
    public async Task Department_not_active_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var department = await CreateParentDepartment(active: false);
        var departmentId = department.Id;        
        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var locationsIds = new[] { locationId1 };
        
        var request = new UpdateLocationsRequest(locationsIds);
        
        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentId}/locations", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            var departmentLocations = await dbContext.DepartmentLocations
                .Where(dl => dl.DepartmentId == departmentId)
                .Select(ld => ld.LocationId)
                .ToListAsync(cancellationToken);
            
            Assert.DoesNotContain(locationId1, departmentLocations);
        });
    }
    
    [Fact]
    public async Task One_of_locations_not_exists_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var department = await CreateParentDepartment();
        var departmentId = department.Id;        
        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var locationsIds = new[] { locationId1, Guid.NewGuid() };
        
        var request = new UpdateLocationsRequest(locationsIds);
        
        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentId}/locations", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
    
    [Fact]
    public async Task Has_duplicate_locations_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var department = await CreateParentDepartment();
        var departmentId = department.Id;        
        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");
        var locationId2 = await CreateLocation("hhhh", "Moscow", "Russia", "Office_2");

        var locationsIds = new[] { locationId1, locationId2, locationId1 };
        
        var request = new UpdateLocationsRequest(locationsIds);
        
        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentId}/locations", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
    
    [Fact]
    public async Task Empty_locations_request_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var department = await CreateParentDepartment();
        var departmentId = department.Id;        
        Guid[] locationsIds = [];
        
        var request = new UpdateLocationsRequest(locationsIds);
        
        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentId}/locations", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }
    
    // private async Task<T> ExecuteHandler<T>(Func<UpdateLocationsHandler, Task<T>> action)
    // {
    //     await using var scopeHandler = Services.CreateAsyncScope();
    //     var handler = scopeHandler.ServiceProvider.GetRequiredService<UpdateLocationsHandler>();
    //     return await action(handler);
    // }
}