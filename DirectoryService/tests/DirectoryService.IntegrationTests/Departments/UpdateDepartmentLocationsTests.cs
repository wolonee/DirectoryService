using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class UpdateDepartmentLocationsTests : DirectoryBaseTests
{
    public UpdateDepartmentLocationsTests(DirectoryTestWebFactory factory)
        : base(factory) { }
    
    [Fact]
    public async Task Update_one_location_should_succeed()
    {
        // arrange
        var departmentId = await CreateDepartment();
        var locationId1 = await CreateLocation("ffff", "Moscow", "Russia", "Office_1");
        var locationId2 = await CreateLocation("dddd", "Moscow", "Russia", "Office_2");
        var locationId3 = await CreateLocation("aaaa", "Moscow", "Russia", "Office_3");

        var locationsIds = new[] { locationId1, locationId2, locationId3 };
        
        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new UpdateLocationsCommand(departmentId, new UpdateLocationsRequest(locationsIds));
            
            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var departmentLocations = await dbContext.DepartmentLocations
                .Where(dl => dl.DepartmentId == departmentId)
                .Select(ld => ld.LocationId)
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(3, departmentLocations.Count);
            Assert.Contains(locationId1, departmentLocations);
            Assert.Contains(locationId2, departmentLocations);
            Assert.Contains(locationId3, departmentLocations);
        });
    }

    public async Task<T> ExecuteHandler<T>(Func<UpdateLocationsHandler, Task<T>> action)
    {
        await using var scopeHandler = Services.CreateAsyncScope();
        var handler = scopeHandler.ServiceProvider.GetRequiredService<UpdateLocationsHandler>();
        return await action(handler);
    }
}