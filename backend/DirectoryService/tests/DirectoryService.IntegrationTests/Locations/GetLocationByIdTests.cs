using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Locations.GetLocationById;

public class GetLocationByIdTests : DirectoryBaseTests
{
    public GetLocationByIdTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLocationById_WhenExists_ReturnsLocation()
    {
        var ct = CancellationToken.None;
        var locationId = await CreateLocationViaApi(
            BuildCreateLocationRequest(
                name: "GetByIdOffice",
                street: "GetByIdStreet",
                city: "Novosibirsk",
                country: "Russia"));

        var response = await AppHttpClient.GetAsync($"/api/location/{locationId}", ct);
        var result = await response.HandleResponseAsync<GetLocationByIdResponse>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(locationId, result.Value.Id);
        Assert.Equal("GetByIdOffice", result.Value.Name);
        Assert.Equal("Novosibirsk", result.Value.City);
        Assert.Equal("GetByIdStreet", result.Value.Street);
        Assert.Equal("Russia", result.Value.Country);
        Assert.Equal("Europe/Moscow", result.Value.Timezone);
    }

    [Fact]
    public async Task GetLocationById_WhenNotFound_Returns404()
    {
        var ct = CancellationToken.None;
        var response = await AppHttpClient.GetAsync($"/api/location/{Guid.NewGuid()}", ct);
        var result = await response.HandleResponseAsync<GetLocationByIdResponse?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.NOT_FOUND);
    }
}
