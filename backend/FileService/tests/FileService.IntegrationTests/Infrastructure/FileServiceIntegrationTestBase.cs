using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FileService.Contracts;
using FileService.Contracts.Features.Simple.InitiateUpload;
using FileService.Domain.S3Entities.Assets;
using Microsoft.EntityFrameworkCore;

namespace FileService.IntegrationTests.Infrastructure;

[Collection(FileServiceIntegrationCollection.Name)]
public abstract class FileServiceIntegrationTestBase : IAsyncLifetime
{
    protected FileServiceIntegrationTestBase(FileServiceTestWebFactory factory)
    {
        Factory = factory;
        AppClient = factory.CreateClient();
        StorageClient = new HttpClient();
    }

    protected FileServiceTestWebFactory Factory { get; }

    protected HttpClient AppClient { get; }

    protected HttpClient StorageClient { get; }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        StorageClient.Dispose();
        await Factory.ResetStateAsync();
    }

    protected static async Task<T> ReadResultAsync<T>(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, json);

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement result = document.RootElement.GetProperty("result");
        Assert.NotEqual(JsonValueKind.Null, result.ValueKind);

        return result.Deserialize<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    protected async Task<InitiateUploadResponse> InitiatePreviewAsync(Guid? targetId = null)
    {
        var request = new InitiateUploadRequest
        {
            FileName = "cover.png",
            ContentType = "image/png",
            Size = 8,
            AssetType = "preview",
            Usage = "course_cover",
            TargetType = "course",
            TargetId = targetId ?? Guid.NewGuid(),
        };

        using HttpResponseMessage response = await AppClient.PostAsJsonAsync("/files/initiate", request);
        return await ReadResultAsync<InitiateUploadResponse>(response);
    }

    protected async Task UploadAsync(PresignedUploadDto upload, byte[] content, string contentType)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, upload.Url)
        {
            Content = new ByteArrayContent(content),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using HttpResponseMessage response = await StorageClient.SendAsync(request);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }

    protected async Task<MediaAsset> GetAssetAsync(Guid fileId) =>
        await Factory.ExecuteInDbAsync(context => context.MediaAssets.SingleAsync(asset => asset.Id == fileId));

}
