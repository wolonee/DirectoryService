using System.Net.Http.Json;
using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Response;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit.Abstractions;

namespace DirectoryService.IntegrationTests;

public class CreateDepartmentTests : DirectoryBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory) { }
    
    // ========== SUCCESS CASES ==========
    [Fact]
    public async Task CreateDepartment_with_one_location_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        
        var locationId = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var request = new CreateDepartmentRequest(
            "Подразделение", 
            "podrazelenie", 
            null, 
            [locationId]);
        
        HttpResponseMessage response = await AppHttpClient.PostAsJsonAsync("/api/departments", request, cancellationToken);

        var result = await response.HandleResponseAsync<Guid>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == result.Value, cancellationToken: cancellationToken);
        
            var departmentLocations = await dbContext.DepartmentLocations
                .FirstAsync(l => l.LocationId == locationId, cancellationToken: cancellationToken);
        
            Assert.NotNull(department);
            Assert.NotNull(departmentLocations);
            Assert.Equal(result.Value, department.Id);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_many_locations_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        
        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");
        var locationId2 = await CreateLocation("dddd", "Moscow", "Russia", "Office_2");
        var locationId3 = await CreateLocation("aaaa", "Moscow", "Russia", "Office_3");

        var locationArray = new[] { locationId1, locationId2, locationId3 };

        var request = new CreateDepartmentRequest(
            "Подразделение", "podrazelenie", null, locationArray);
        
        HttpResponseMessage response = await AppHttpClient.PostAsJsonAsync("/api/departments", request, cancellationToken);
        
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == result.Value, cancellationToken: cancellationToken);

            var locations = dbContext.DepartmentLocations
                .Where(x => locationArray.Contains(x.LocationId) && x.DepartmentId == result.Value)
                .ToList();
            
            Assert.NotNull(department);
            Assert.NotNull(locations);
            Assert.Equal(locations.Count, locationArray.Length);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_parent_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationId = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");
        var parentDepartment = await CreateParentDepartment();
        var parentDepartmentId = parentDepartment.Id;

        var request = new CreateDepartmentRequest(
            "ChildDepartment", "child_department", parentDepartmentId, [locationId]);
        
        var response = await AppHttpClient.PostAsJsonAsync("/api/departments", request, cancellationToken);
        
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == result.Value, cancellationToken: cancellationToken);
            
            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDepartmentId, cancellationToken: cancellationToken);
            
            Assert.NotNull(department);
            Assert.NotNull(parentDepartment);
            
            Assert.Equal(department.ParentId, parentDepartment.Id);
            Assert.Equal(0, parentDepartment.Depth);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, parentDepartment.Id);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_invalid_name_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationId = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var request = new CreateDepartmentRequest(
            "d", "identifier", null, [locationId]);
        
        var response = await AppHttpClient.PostAsJsonAsync("/api/departments", request, cancellationToken);
        
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
        
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.DepartmentIdentifier.Value == "identifier", cancellationToken: cancellationToken);
            
            Assert.Null(department);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_invalid_identifier_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationId = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var request = new CreateDepartmentRequest(
            "department", "_identifier", null, [locationId]);
        
        var response = await AppHttpClient.PostAsJsonAsync("/api/departments", request, cancellationToken);
        
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
        
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName == DepartmentName.Create("department").Value, cancellationToken: cancellationToken);
            
            Assert.Null(department);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_invalid_parentId_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationId = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");

        var request = new CreateDepartmentRequest(
            "department", "department", Guid.Empty, [locationId]);
        
        var response = await AppHttpClient.PostAsJsonAsync("/api/departments", request, cancellationToken);
        
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName == DepartmentName.Create("department").Value, cancellationToken);
        
            Assert.Null(department);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_empty_locationId_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new CreateDepartmentRequest(
            "department", "department", null, [Guid.Empty]);
        
        var response = await AppHttpClient.PostAsJsonAsync("/api/departments", request, cancellationToken);
        
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
        
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName == DepartmentName.Create("department").Value, cancellationToken);
        
            var departmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dp => dp.LocationId == Guid.Empty, cancellationToken);
            
            Assert.Null(departmentLocations);
            Assert.Null(department);
        });
    }
    
    // Extensions
    // private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    // {
    //     await using var scope = Services.CreateAsyncScope();
    //     var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
    //     return await action(sut);
    // }
}