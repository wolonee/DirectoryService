using System.Net.Http.Json;
using System.Text.Json;
using FileService.Contracts;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;

namespace FileService.IntegrationTests;

public sealed class MultipartUploadFlowTests(FileServiceTestWebFactory factory) : FileServiceIntegrationTestBase(factory)
{
    private const int PartSize = 5 * 1024 * 1024;

    [Fact]
    public async Task StartUploadPartsComplete_SavesReadyAssetAndFinalObject()
    {
        StartMultipartUploadResponse started = await StartAsync();
        Assert.Equal(PartSize, started.ChunkSize);
        Assert.Equal(2, started.TotalChunks);

        var partEtags = new List<PartETagDto>();
        foreach (MultipartPartUploadDto part in started.Parts)
        {
            using var content = new ByteArrayContent(CreatePartContent(part.PartNumber));
            using HttpResponseMessage response = await StorageClient.PutAsync(part.UploadUrl, content);
            Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
            partEtags.Add(new PartETagDto(part.PartNumber, response.Headers.ETag!.Tag));
        }

        var completeRequest = new CompleteMultipartUploadRequest
        {
            FileId = started.FileId,
            UploadId = started.UploadId,
            Parts = partEtags,
        };
        using HttpResponseMessage completeResponse = await AppClient.PostAsJsonAsync(
            "/files/multipart/complete",
            completeRequest);
        CompleteMultipartUploadResponse completed = await ReadResultAsync<CompleteMultipartUploadResponse>(completeResponse);
        Assert.Equal(started.FileId, completed.FileId);

        MediaAsset asset = await GetAssetAsync(started.FileId);
        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.NotNull(asset.StorageReference);
        Assert.Equal(PartSize * 2L, asset.StorageReference.Size);
    }

    [Fact]
    public async Task CompleteWithWrongParts_ReturnsFailureAndKeepsMultipartActive()
    {
        StartMultipartUploadResponse started = await StartAsync();
        var invalidRequest = new CompleteMultipartUploadRequest
        {
            FileId = started.FileId,
            UploadId = started.UploadId,
            Parts = [new PartETagDto(1, "fake-etag")],
        };

        using HttpResponseMessage response = await AppClient.PostAsJsonAsync("/files/multipart/complete", invalidRequest);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("result").ValueKind);
        Assert.Equal(MediaStatus.UPLOADING, (await GetAssetAsync(started.FileId)).Status);
    }

    [Fact]
    public async Task AbortAfterPartialUpload_AbortsStorageSessionAndDeletesAsset()
    {
        StartMultipartUploadResponse started = await StartAsync();
        using (var content = new ByteArrayContent(CreatePartContent(1)))
        using (HttpResponseMessage response = await StorageClient.PutAsync(started.Parts[0].UploadUrl, content))
            Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

        using HttpResponseMessage abortResponse = await AppClient.PostAsJsonAsync(
            "/files/multipart/abort",
            new AbortMultipartUploadRequest
            {
                FileId = started.FileId,
                UploadId = started.UploadId,
            });
        AbortMultipartUploadResponse aborted = await ReadResultAsync<AbortMultipartUploadResponse>(abortResponse);
        Assert.Equal("deleted", aborted.Status);
        Assert.Equal(MediaStatus.DELETED, (await GetAssetAsync(started.FileId)).Status);
    }

    private static byte[] CreatePartContent(int value)
    {
        byte[] content = new byte[PartSize];
        content[0] = (byte)value;
        return content;
    }

    private async Task<StartMultipartUploadResponse> StartAsync()
    {
        var request = new StartMultipartUploadRequest
        {
            FileName = "lesson.mp4",
            ContentType = "video/mp4",
            Size = PartSize * 2L,
            AssetType = "video",
            Usage = "lesson_video",
            TargetType = "lesson",
            TargetId = Guid.NewGuid(),
        };

        using HttpResponseMessage response = await AppClient.PostAsJsonAsync("/files/multipart/start", request);
        return await ReadResultAsync<StartMultipartUploadResponse>(response);
    }

}
