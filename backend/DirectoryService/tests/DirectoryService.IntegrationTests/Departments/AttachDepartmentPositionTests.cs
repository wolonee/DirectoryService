using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments.AttachDepartmentPosition;

public class AttachDepartmentPositionTests : DirectoryBaseTests
{
    public AttachDepartmentPositionTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task PostDepartmentPosition_WhenValid_AttachesLink()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionInDb("Backend", "Dev", ct);

        var response = await AppHttpClient.PostAsync(
            $"/api/departments/{department.Id}/positions/{positionId}",
            null,
            ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            Assert.True(await db.DepartmentPositions.AnyAsync(
                dp => dp.DepartmentId == department.Id && dp.PositionId == positionId,
                ct));
        });
    }

    [Fact]
    public async Task PostDepartmentPosition_WhenAlreadyLinked_Returns409()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionInDb("Backend", "Dev", ct);

        var first = await AppHttpClient.PostAsync(
            $"/api/departments/{department.Id}/positions/{positionId}",
            null,
            ct);
        Assert.True((await first.HandleResponseAsync(ct)).IsSuccess);

        var second = await AppHttpClient.PostAsync(
            $"/api/departments/{department.Id}/positions/{positionId}",
            null,
            ct);
        var result = await second.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.CONFLICT);
    }

    [Fact]
    public async Task PostDepartmentPosition_WhenDepartmentNotFound_Returns404()
    {
        var ct = CancellationToken.None;
        var positionId = await CreatePositionInDb("QA", "Lead", ct);

        var response = await AppHttpClient.PostAsync(
            $"/api/departments/{Guid.NewGuid()}/positions/{positionId}",
            null,
            ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }
}
