using System.Net;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Communications.Communication.HttpCommunication;
using FileService.Contracts;
using FileService.Contracts.Features.Simple.GetMediaAsset;
using FileService.Contracts.Features.Simple.GetMediaAssets;
using FileService.Contracts.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileService.Communications.UnitTests;

public sealed class FileCommunicationServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetMediaAsset_WithSuccessfulEnvelope_ReturnsTypedResponse()
    {
        Guid fileId = Guid.CreateVersion7();
        GetMediaAssetResponse expected = CreateMediaAssetResponse(fileId);
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/files/{fileId}", request.RequestUri?.PathAndQuery);
            return Task.FromResult(CreateResponse(HttpStatusCode.OK, Envelope<GetMediaAssetResponse>.Ok(expected)));
        });
        FileCommunicationService client = CreateClient(handler);

        Result<GetMediaAssetResponse, Errors> result = await client.GetMediaAsset(
            new GetMediaAssetRequest(fileId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task GetMediaAssetsByIds_WithSuccessfulEnvelope_SerializesRequestAndReturnsResponse()
    {
        Guid fileId = Guid.CreateVersion7();
        var expected = new GetMediaAssetsResponse([
            new GetMediaAssetDto(fileId, "ready", "image/png", "https://storage.test/file"),
        ]);
        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/files", request.RequestUri?.PathAndQuery);

            string body = await request.Content!.ReadAsStringAsync(cancellationToken);
            GetMediaAssetsRequest? actual = JsonSerializer.Deserialize<GetMediaAssetsRequest>(body, JsonOptions);
            Assert.Equal([fileId], actual?.FileIds);

            return CreateResponse(HttpStatusCode.OK, Envelope<GetMediaAssetsResponse>.Ok(expected));
        });
        FileCommunicationService client = CreateClient(handler);

        Result<GetMediaAssetsResponse, Errors> result = await client.GetMediaAssetsByIds(
            new GetMediaAssetsRequest([fileId]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        GetMediaAssetDto mediaAsset = Assert.Single(result.Value.MediaAssets);
        Assert.Equal(fileId, mediaAsset.Id);
        Assert.Equal("ready", mediaAsset.Status);
        Assert.Equal("image/png", mediaAsset.ContentType);
        Assert.Equal("https://storage.test/file", mediaAsset.ContentUrl);
    }

    [Fact]
    public async Task GetMediaAsset_WithNotFoundEnvelope_PreservesServerError()
    {
        Error notFound = Error.NotFound("file.not.found", "File was not found.");
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(
            CreateResponse(
                HttpStatusCode.NotFound,
                Envelope<GetMediaAssetResponse>.Errors(notFound.ToErrors()))));
        FileCommunicationService client = CreateClient(handler);

        Result<GetMediaAssetResponse, Errors> result = await client.GetMediaAsset(
            new GetMediaAssetRequest(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("file.not.found", Assert.Single(result.Error).Code);
    }

    [Fact]
    public async Task GetMediaAsset_WithNetworkFailure_ReturnsUnavailableError()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            throw new HttpRequestException("Host is unavailable."));
        FileCommunicationService client = CreateClient(handler);

        Result<GetMediaAssetResponse, Errors> result = await client.GetMediaAsset(
            new GetMediaAssetRequest(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("file.service.unavailable", Assert.Single(result.Error).Code);
    }

    [Fact]
    public async Task GetMediaAsset_WithClientTimeout_ReturnsTimeoutError()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            throw new TaskCanceledException("Request timed out."));
        FileCommunicationService client = CreateClient(handler);

        Result<GetMediaAssetResponse, Errors> result = await client.GetMediaAsset(
            new GetMediaAssetRequest(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("file.service.timeout", Assert.Single(result.Error).Code);
    }

    [Fact]
    public async Task GetMediaAsset_WithCallerCancellation_PropagatesCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var handler = new StubHttpMessageHandler((_, cancellationToken) =>
            throw new OperationCanceledException(cancellationToken));
        FileCommunicationService client = CreateClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetMediaAsset(
            new GetMediaAssetRequest(Guid.CreateVersion7()),
            cancellationTokenSource.Token));
    }

    private static FileCommunicationService CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://files.test"),
        };

        return new FileCommunicationService(
            httpClient,
            NullLogger<FileCommunicationService>.Instance);
    }

    private static HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, Envelope<T> envelope)
    {
        string json = JsonSerializer.Serialize(envelope, JsonOptions);

        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private static GetMediaAssetResponse CreateMediaAssetResponse(Guid fileId) => new(
        fileId,
        Guid.CreateVersion7(),
        "course",
        "ready",
        "preview",
        "image/png",
        "course-cover",
        1024,
        new ObjectMetadataDto(1024, "image/png", "etag", null, DateTime.UnixEpoch),
        "https://storage.test/file");

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request, cancellationToken);
    }
}
