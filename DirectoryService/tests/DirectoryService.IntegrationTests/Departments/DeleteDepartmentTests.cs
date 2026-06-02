using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments.DeleteDepartment;

public class DeleteDepartmentTests : DirectoryBaseTests
{
    public DeleteDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteDepartment_WhenNoActiveChildren_Deactivates()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();

        var response = await AppHttpClient.DeleteAsync($"/api/departments/{department.Id}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            var entity = await db.Departments.FirstAsync(d => d.Id == department.Id, ct);
            Assert.False(entity.IsActive);
            Assert.False(await db.DepartmentPositions.AnyAsync(dp => dp.DepartmentId == department.Id, ct));
            Assert.False(await db.DepartmentLocations.AnyAsync(dl => dl.DepartmentId == department.Id, ct));
        });
    }

    [Fact]
    public async Task DeleteDepartment_WithActiveChild_Returns409()
    {
        var ct = CancellationToken.None;
        var parent = await CreateParentDepartment();
        await CreateChildDepartment(parent, difference: "child-for-delete-test");

        var response = await AppHttpClient.DeleteAsync($"/api/departments/{parent.Id}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.CONFLICT);
    }

    [Fact]
    public async Task DeleteDepartment_WhenNotFound_Returns404()
    {
        var ct = CancellationToken.None;
        var response = await AppHttpClient.DeleteAsync($"/api/departments/{Guid.NewGuid()}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }
}
