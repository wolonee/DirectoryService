using System.Net.Http.Json;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Positions.RenamePosition;

public class RenamePositionTests : DirectoryBaseTests
{
    public RenamePositionTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task PatchPosition_WhenValid_UpdatesName()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionViaApi(department.Id, "Old", "Role", ct);

        var request = new RenamePositionRequest(new CreatePositionNameRequest("New", "Role"));
        var response = await AppHttpClient.PatchAsJsonAsync($"/api/positions/{positionId}", request, ct);
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: ct);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            var position = await db.Positions.FirstAsync(p => p.Id == positionId, ct);
            Assert.Equal("New", position.Name.Speciality);
            Assert.Equal("Role", position.Name.Direction);
        });
    }

    [Fact]
    public async Task PatchPosition_WhenNotFound_Returns404()
    {
        var ct = CancellationToken.None;
        var request = new RenamePositionRequest(new CreatePositionNameRequest("New", "Role"));

        var response = await AppHttpClient.PatchAsJsonAsync($"/api/positions/{Guid.NewGuid()}", request, ct);
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task PatchPosition_WithDuplicateActiveName_Returns409()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        await CreatePositionViaApi(department.Id, "Taken", "Role", ct);
        var positionId = await CreatePositionViaApi(department.Id, "Other", "Role", ct);

        var request = new RenamePositionRequest(new CreatePositionNameRequest("Taken", "Role"));
        var response = await AppHttpClient.PatchAsJsonAsync($"/api/positions/{positionId}", request, ct);
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.CONFLICT);
    }

    [Fact]
    public async Task PatchPosition_WithInvalidName_Returns400()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionViaApi(department.Id, "Valid", "Role", ct);

        var request = new RenamePositionRequest(new CreatePositionNameRequest("x", "y"));
        var response = await AppHttpClient.PatchAsJsonAsync($"/api/positions/{positionId}", request, ct);
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.VALIDATION);
    }
}
