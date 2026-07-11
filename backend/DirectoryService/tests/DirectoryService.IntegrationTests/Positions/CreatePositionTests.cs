using System.Net.Http.Json;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Positions.CreatePosition;

public class CreatePositionTests : DirectoryBaseTests
{
    public CreatePositionTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task PostPosition_WhenValid_ReturnsId()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();

        var request = new CreatePositionRequest(
            new CreatePositionNameRequest("Developer", "Backend"),
            "Description",
            [department.Id]);

        var response = await AppHttpClient.PostAsJsonAsync("/api/positions", request, ct);
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async db =>
        {
            var position = await db.Positions.FirstAsync(p => p.Id == result.Value, ct);
            Assert.True(position.IsActive);
            Assert.True(await db.DepartmentPositions.AnyAsync(
                dp => dp.DepartmentId == department.Id && dp.PositionId == result.Value,
                ct));
        });
    }

    [Fact]
    public async Task PostPosition_WithDuplicateActiveName_Returns409()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();

        var request = new CreatePositionRequest(
            new CreatePositionNameRequest("Developer", "Backend"),
            null,
            [department.Id]);

        var first = await AppHttpClient.PostAsJsonAsync("/api/positions", request, ct);
        Assert.True((await first.HandleResponseAsync<Guid>(cancellationToken: ct)).IsSuccess);

        var second = await AppHttpClient.PostAsJsonAsync("/api/positions", request, ct);
        var result = await second.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.CONFLICT);
    }

    [Fact]
    public async Task PostPosition_WhenDepartmentNotFound_Returns404()
    {
        var ct = CancellationToken.None;

        var request = new CreatePositionRequest(
            new CreatePositionNameRequest("Developer", "Backend"),
            null,
            [Guid.NewGuid()]);

        var response = await AppHttpClient.PostAsJsonAsync("/api/positions", request, ct);
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task PostPosition_WithDuplicatedDepartmentIds_Returns400()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();

        var request = new CreatePositionRequest(
            new CreatePositionNameRequest("Developer", "Backend"),
            null,
            [department.Id, department.Id]);

        var response = await AppHttpClient.PostAsJsonAsync("/api/positions", request, ct);
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.VALIDATION);
    }
}
