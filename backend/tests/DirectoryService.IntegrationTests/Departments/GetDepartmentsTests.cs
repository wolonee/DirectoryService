using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentsTests : DirectoryBaseTests
{
    public GetDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartments_WhenDepartmentsExist_ReturnsPagedList()
    {
        var ct = CancellationToken.None;
        var parent = await CreateParentDepartment();
        await CreateChildDepartment(parent, difference: "ListChild");

        var response = await AppHttpClient.GetAsync(
            "/api/departments?Pagination.Page=1&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetDepartmentsResponse>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TotalCount >= 2);
        Assert.Contains(result.Value.Departments, d => d.Name == "ParentDepartment");
        Assert.Contains(result.Value.Departments, d => d.Name == "listchildDepartmentName");
    }

    [Fact]
    public async Task GetDepartments_WithSearchFilter_ReturnsMatchingDepartments()
    {
        var ct = CancellationToken.None;
        var parent = await CreateParentDepartment();
        await CreateChildDepartment(parent, difference: "UniqueSearchXYZ");

        const string searchTerm = "uniquesearchxyz";
        var response = await AppHttpClient.GetAsync(
            $"/api/departments?Search={searchTerm}&Pagination.Page=1&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetDepartmentsResponse>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Departments);
        Assert.All(
            result.Value.Departments,
            d => Assert.Contains(searchTerm, d.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDepartments_WithIsActiveFalse_ReturnsOnlyInactive()
    {
        var ct = CancellationToken.None;
        var parent = await CreateParentDepartment();
        await CreateChildDepartment(parent, difference: "InactiveChild", active: false);

        var response = await AppHttpClient.GetAsync(
            "/api/departments?IsActive=false&Pagination.Page=1&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetDepartmentsResponse>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Departments, d => d.Name == "inactivechildDepartmentName");
        Assert.DoesNotContain(result.Value.Departments, d => d.Name == "ParentDepartment");
    }

    [Fact]
    public async Task GetDepartments_WithLocationIdsFilter_ReturnsLinkedDepartments()
    {
        var ct = CancellationToken.None;
        var locationId = await CreateLocation("FilterStreet", "Moscow", "Russia", "FilterOffice");
        var parent = await CreateParentDepartment();
        await AttachLocationToDepartment(parent.Id, locationId, ct);
        await CreateChildDepartment(parent, difference: "NoLink");

        var response = await AppHttpClient.GetAsync(
            $"/api/departments?LocationIds={locationId}&Pagination.Page=1&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetDepartmentsResponse>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Departments, d => d.Name == "ParentDepartment");
        Assert.DoesNotContain(result.Value.Departments, d => d.Name == "nolinkDepartmentName");
    }

    [Fact]
    public async Task GetDepartments_WithInvalidPagination_Returns400()
    {
        var ct = CancellationToken.None;
        var response = await AppHttpClient.GetAsync(
            "/api/departments?Pagination.Page=0&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetDepartmentsResponse?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.VALIDATION);
    }
}
