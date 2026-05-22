using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        var locationId = await CreateLocation();
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                "Подразделение", "podrazelenie", null, [locationId]));

            // act
            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == result.Value, cancellationToken: cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, result.Value);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_many_locations_should_succeed()
    {
        // arrange
        var locationId1 = await CreateLocation();
        var locationId2 = await CreateLocation();
        var locationId3 = await CreateLocation();

        var locationArray = new[] { locationId1, locationId2, locationId3 };
        
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                "Подразделение", "podrazelenie", null, locationArray));
        
            // act
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == result.Value, cancellationToken: cancellationToken);

            var locations = dbContext.DepartmentLocations
                .Where(x => locationArray.Contains(x.LocationId) && x.DepartmentId == result.Value)
                .ToList();
            
            Assert.NotNull(department);
            Assert.NotNull(locations);
            Assert.Equal(locations.Count, locationArray.Length);
            
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_parent_should_succeed()
    {
        // arrange
        var locationId = await CreateLocation();
        var parentDepartmentId = await CreateDepartment();
        
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                "ChildDepartment", "child.department", parentDepartmentId, [locationId]));
        
            // act
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == result.Value, cancellationToken: cancellationToken);
            
            var parentDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == parentDepartmentId, cancellationToken: cancellationToken);
            
            Assert.NotNull(department);
            Assert.NotNull(parentDepartment);
            
            Assert.Equal(department.ParentId, parentDepartment.Id);
            
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            Assert.NotEqual(Guid.Empty, parentDepartment.Id);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_invalid_data_should_failed()
    {
        // arrange
        var locationId = await CreateLocation();
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                "d", "_department", null, [locationId]));
        
            // act
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task CreateDepartment_with_invalid_parentId_should_failed()
    {
        // arrange
        var locationId = await CreateLocation();
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                "department", "department", Guid.Empty, [locationId]));
        
            // act
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName == DepartmentName.Create("department").Value, cancellationToken);
        
            Assert.Null(department);
        });
    }

    private async Task<Guid> CreateLocation()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                LocationAddress.Create("pepe", "Togliatti", "Russia").Value,
                LocationName.Create("Локация").Value,
                LocationTimeZone.Create("Europe/Moscow").Value).Value;

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return location.Id;
        });
    }
    
    private async Task<Guid> CreateDepartment()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var locationId = await CreateLocation();
            
            var departmentId = Guid.NewGuid();
            var departmentLocations = DepartmentLocation.Create(departmentId, locationId).Value;

            var department = Department.CreateParent(
                DepartmentName.Create("ParentDepartment").Value,
                DepartmentIdentifier.Create("parent").Value,
                [departmentLocations],
                departmentId).Value;

            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();

            return department.Id;
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await action(sut);
    }
}