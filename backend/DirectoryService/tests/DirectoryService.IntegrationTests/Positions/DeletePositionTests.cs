using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Positions.DeletePosition;

public class DeletePositionTests : DirectoryBaseTests
{
    public DeletePositionTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeletePosition_WhenExists_DeactivatesAndRemovesLinks()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionViaApi(department.Id, "Frontend", "Dev", ct);

        var response = await AppHttpClient.DeleteAsync($"/api/positions/{positionId}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            var position = await db.Positions.FirstAsync(p => p.Id == positionId, ct);
            Assert.False(position.IsActive);
            Assert.False(await db.DepartmentPositions.AnyAsync(dp => dp.PositionId == positionId, ct));
        });
    }

    [Fact]
    public async Task DeletePosition_WhenNotFound_Returns404()
    {
        var ct = CancellationToken.None;
        var response = await AppHttpClient.DeleteAsync($"/api/positions/{Guid.NewGuid()}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task DeletePosition_WhenAlreadyInactive_Returns409()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionViaApi(department.Id, "Backend", "Lead", ct);

        var first = await AppHttpClient.DeleteAsync($"/api/positions/{positionId}", ct);
        Assert.True((await first.HandleResponseAsync(ct)).IsSuccess);

        var second = await AppHttpClient.DeleteAsync($"/api/positions/{positionId}", ct);
        var result = await second.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.CONFLICT);
    }
}
