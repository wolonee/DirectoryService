using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Locations.DeleteLocation;

public class DeleteLocationTests : DirectoryBaseTests
{
    public DeleteLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteLocation_WhenExists_DeactivatesAndRemovesLinks()
    {
        var ct = CancellationToken.None;
        var locationId = await CreateLocation("del", "Moscow", "Russia", "DelOffice");
        var department = await CreateParentDepartment();
        await AttachLocationToDepartment(department.Id, locationId, ct);

        var response = await AppHttpClient.DeleteAsync($"/api/location/{locationId}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            var location = await db.Locations.FirstAsync(l => l.Id == locationId, ct);
            Assert.False(location.IsActive);
            Assert.False(await db.DepartmentLocations.AnyAsync(dl => dl.LocationId == locationId, ct));
        });
    }

    [Fact]
    public async Task DeleteLocation_WhenNotFound_Returns404()
    {
        var ct = CancellationToken.None;
        var response = await AppHttpClient.DeleteAsync($"/api/location/{Guid.NewGuid()}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }
}
