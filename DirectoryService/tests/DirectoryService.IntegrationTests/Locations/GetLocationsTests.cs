using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Locations.GetLocations;

public class GetLocationsTests : DirectoryBaseTests
{
    public GetLocationsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLocations_WhenLocationsExist_ReturnsPagedList()
    {
        var ct = CancellationToken.None;
        await CreateLocationViaApi(BuildCreateLocationRequest(name: "ListOfficeOne", street: "ListStreet1"));
        await CreateLocationViaApi(BuildCreateLocationRequest(name: "ListOfficeTwo", street: "ListStreet2"));

        var response = await AppHttpClient.GetAsync(
            "/api/location?Pagination.Page=1&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetLocationsResponse>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TotalCount >= 2);
        Assert.Contains(result.Value.Locations, l => l.Name == "ListOfficeOne");
        Assert.Contains(result.Value.Locations, l => l.Name == "ListOfficeTwo");
    }

    [Fact]
    public async Task GetLocations_WithSearchFilter_ReturnsMatchingLocations()
    {
        var ct = CancellationToken.None;
        const string uniqueName = "UniqueSearchOfficeXYZ";
        await CreateLocationViaApi(BuildCreateLocationRequest(name: uniqueName, street: "SearchStreet"));
        await CreateLocationViaApi(BuildCreateLocationRequest(name: "OtherOffice", street: "OtherSearchStreet"));

        var response = await AppHttpClient.GetAsync(
            $"/api/location?Search={uniqueName}&Pagination.Page=1&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetLocationsResponse>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value.Locations, l => Assert.Contains(uniqueName, l.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetLocations_WithInvalidPagination_Returns400()
    {
        var ct = CancellationToken.None;
        var response = await AppHttpClient.GetAsync(
            "/api/location?Pagination.Page=0&Pagination.PageSize=20",
            ct);
        var result = await response.HandleResponseAsync<GetLocationsResponse?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.VALIDATION);
    }
}
