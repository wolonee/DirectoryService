using System.Net.Http.Json;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Locations.CreateLocation;

public class CreateLocationTests : DirectoryBaseTests
{
    public CreateLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task PostLocation_WhenValid_ReturnsId()
    {
        var ct = CancellationToken.None;
        var request = BuildCreateLocationRequest(
            name: "NewOffice",
            street: "Lenina 1",
            city: "Kazan",
            country: "Russia");

        var response = await AppHttpClient.PostAsJsonAsync("/api/location", request, ct);
        var result = await response.HandleResponseAsync<Guid>(cancellationToken: ct);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async db =>
        {
            var location = await db.Locations.FirstAsync(l => l.Id == result.Value, ct);
            Assert.Equal("NewOffice", location.Name.Value);
            Assert.Equal("Kazan", location.Address.City);
            Assert.True(location.IsActive);
        });
    }

    [Fact]
    public async Task PostLocation_WithDuplicateName_Returns409()
    {
        var ct = CancellationToken.None;
        var request = BuildCreateLocationRequest(name: "DuplicateNameOffice", street: "Street1");

        var first = await AppHttpClient.PostAsJsonAsync("/api/location", request, ct);
        Assert.True((await first.HandleResponseAsync<Guid>(cancellationToken: ct)).IsSuccess);

        var duplicate = request with { Address = new CreateLocationAddressRequest("Russia", "SPb", "OtherStreet") };
        var second = await AppHttpClient.PostAsJsonAsync("/api/location", duplicate, ct);
        var result = await second.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.CONFLICT);
    }

    [Fact]
    public async Task PostLocation_WithDuplicateAddress_Returns409()
    {
        var ct = CancellationToken.None;
        var address = new CreateLocationAddressRequest("Russia", "Moscow", "SharedAddress");
        var firstRequest = BuildCreateLocationRequest(name: "OfficeA", street: address.Street, city: address.City, country: address.Country);
        var secondRequest = BuildCreateLocationRequest(name: "OfficeB", street: address.Street, city: address.City, country: address.Country);

        var first = await AppHttpClient.PostAsJsonAsync("/api/location", firstRequest, ct);
        Assert.True((await first.HandleResponseAsync<Guid>(cancellationToken: ct)).IsSuccess);

        var second = await AppHttpClient.PostAsJsonAsync("/api/location", secondRequest, ct);
        var result = await second.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.CONFLICT);
    }

    [Fact]
    public async Task PostLocation_WithInvalidName_Returns400()
    {
        var ct = CancellationToken.None;
        var request = BuildCreateLocationRequest(name: "ab");

        var response = await AppHttpClient.PostAsJsonAsync("/api/location", request, ct);
        var result = await response.HandleResponseAsync<Guid?>(cancellationToken: ct);

        Assert.True(result.IsFailure);
        AssertErrorType(result.Error, ErrorType.VALIDATION);
    }
}
