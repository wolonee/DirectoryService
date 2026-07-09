using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments.DetachDepartmentPosition;

public class DetachDepartmentPositionTests : DirectoryBaseTests
{
    public DetachDepartmentPositionTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteDepartmentPosition_WhenLinked_RemovesLink()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionViaApi(department.Id, "Frontend", "Dev", ct);

        var response = await AppHttpClient.DeleteAsync(
            $"/api/departments/{department.Id}/positions/{positionId}",
            ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            Assert.False(await db.DepartmentPositions.AnyAsync(
                dp => dp.DepartmentId == department.Id && dp.PositionId == positionId,
                ct));
        });
    }

    [Fact]
    public async Task DeleteDepartmentPosition_WhenNotLinked_Returns404()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionInDb("QA", "Engineer", ct);

        var response = await AppHttpClient.DeleteAsync(
            $"/api/departments/{department.Id}/positions/{positionId}",
            ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }
}
