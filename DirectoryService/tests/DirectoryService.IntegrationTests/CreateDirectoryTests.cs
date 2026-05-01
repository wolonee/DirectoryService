using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class CreateDirectoryTests : IClassFixture<DirectoryTestWebFactory>
{
    private IServiceProvider Services { get; set; }
    
    public CreateDirectoryTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
    }
    
    [Fact]
    public async Task CreateDirectory_with_valid_data_should_succeed()
    {
        // arrange
        Guid locationId = await CreateLocation();

        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        
        var cancellationToken = CancellationToken.None;

        var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
            "Подразделение", "podrazelenie", null, [locationId]));
        
        // act
        var result = await sut.Handle(command, cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }
    
    [Fact]
    public async Task CreateDirectory_with_valid_data_should_failed()
    {
        // arrange
        Guid locationId = await CreateLocation();

        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        
        var cancellationToken = CancellationToken.None;

        var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
            "a", "podrazelenie", null, [locationId]));
        
        // act
        var result = await sut.Handle(command, cancellationToken);
        
        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<Guid> CreateLocation()
    {
        await using var initializerScope = Services.CreateAsyncScope();
        var dbContext = initializerScope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        var location = Location.Create(
            LocationAddress.Create("pepe", "Togliatti", "Russia").Value,
            LocationName.Create("Локация").Value,
            LocationTimeZone.Create("Europe/Moscow").Value).Value;
            
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();
            
        return location.Id;
    }
}