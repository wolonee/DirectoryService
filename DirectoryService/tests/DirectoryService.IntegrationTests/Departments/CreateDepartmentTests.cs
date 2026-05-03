using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
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
    
    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
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
    public async Task CreateDepartment_with_invalid_name_should_failed()
    {
        // arrange
        var locationId = await CreateLocation();
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                "a", "podrazelenie", null, [locationId]));
        
            // act
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
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
    
    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await action(sut);
    }
}