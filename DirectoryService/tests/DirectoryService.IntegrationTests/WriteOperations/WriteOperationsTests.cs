using System.Net.Http.Json;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.WriteOperations;

public class WriteOperationsTests : DirectoryBaseTests
{
    public WriteOperationsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteLocation_when_exists_should_deactivate_and_remove_links()
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
    public async Task DeleteLocation_when_not_found_should_fail()
    {
        var ct = CancellationToken.None;
        var response = await AppHttpClient.DeleteAsync($"/api/location/{Guid.NewGuid()}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DeleteDepartment_with_active_child_should_conflict()
    {
        var ct = CancellationToken.None;
        var parent = await CreateParentDepartment();
        await CreateChildDepartment(parent, difference: "child-for-delete-test");

        var response = await AppHttpClient.DeleteAsync($"/api/departments/{parent.Id}", ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AttachPosition_twice_should_conflict()
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
    }

    [Fact]
    public async Task DetachPosition_when_not_linked_should_fail()
    {
        var ct = CancellationToken.None;
        var department = await CreateParentDepartment();
        var positionId = await CreatePositionInDb("QA", "Engineer", ct);

        var response = await AppHttpClient.DeleteAsync(
            $"/api/departments/{department.Id}/positions/{positionId}",
            ct);
        var result = await response.HandleResponseAsync(ct);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DeletePosition_should_remove_department_links()
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
    public async Task RenamePosition_should_update_name()
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

    private async Task<Guid> CreatePositionViaApi(
        Guid departmentId,
        string speciality,
        string direction,
        CancellationToken ct)
    {
        var request = new CreatePositionRequest(
            new CreatePositionNameRequest(speciality, direction),
            null,
            [departmentId]);

        var response = await AppHttpClient.PostAsJsonAsync("/api/positions", request, ct);
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: ct);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private async Task<Guid> CreatePositionInDb(string speciality, string direction, CancellationToken ct)
    {
        return await ExecuteInDb(async db =>
        {
            var position = Position.Create(
                Guid.NewGuid(),
                PositionName.Create(speciality, direction).Value,
                null).Value;

            db.Positions.Add(position);
            await db.SaveChangesAsync(ct);
            return position.Id;
        });
    }

    private async Task AttachLocationToDepartment(Guid departmentId, Guid locationId, CancellationToken ct)
    {
        await ExecuteInDb(async db =>
        {
            var link = DepartmentLocation.Create(departmentId, locationId).Value;
            db.DepartmentLocations.Add(link);
            await db.SaveChangesAsync(ct);
        });
    }
}
