using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Amazon.S3.Model;
using FileService.Contracts;
using FileService.Contracts.Features.Simple.CompleteUpload;
using FileService.Contracts.Features.Simple.GetMediaAsset;
using FileService.Contracts.Features.Simple.InitiateUpload;
using FileService.Domain;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FileService.IntegrationTests;

public sealed class SimpleUploadFlowTests(FileServiceTestWebFactory factory) : FileServiceIntegrationTestBase(factory)
{
    private static readonly byte[] ImageBytes = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public async Task InitiatePutCompleteGet_StoresMatchingDatabaseAndMinioObject()
    {
        InitiateUploadResponse initiated = await InitiatePreviewAsync();

        await UploadAsync(initiated.Upload, ImageBytes, "image/png");

        using HttpResponseMessage completeResponse = await AppClient.PostAsync(
            $"/files/{initiated.FileId}/complete",
            content: null);
        CompleteUploadResponse completed = await ReadResultAsync<CompleteUploadResponse>(completeResponse);
        Assert.Equal(initiated.FileId, completed.FileId);
        Assert.Equal("READY", completed.Status);

        using HttpResponseMessage getResponse = await AppClient.GetAsync($"/files/{initiated.FileId}");
        GetMediaAssetResponse file = await ReadResultAsync<GetMediaAssetResponse>(getResponse);
        Assert.Equal("ready", file.Status);
        Assert.NotNull(file.ContentUrl);
        Assert.NotNull(file.Storage);
        Assert.Equal(ImageBytes.Length, file.Storage.ContentLength);

        byte[] downloaded = await StorageClient.GetByteArrayAsync(file.ContentUrl!);
        Assert.Equal(ImageBytes, downloaded);

        var asset = await Factory.ExecuteInDbAsync(context => context.MediaAssets
            .Include(item => item.StorageReference)
            .SingleAsync(item => item.Id == initiated.FileId));
        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.NotNull(asset.StorageReference);
        Assert.Equal(ImageBytes.Length, asset.StorageReference.Size);

        GetObjectMetadataResponse metadata = await Factory.S3Client.GetObjectMetadataAsync(
            asset.UploadKey.Bucket,
            asset.UploadKey.Value);
        Assert.Equal(ImageBytes.Length, metadata.Headers.ContentLength);
        Assert.Equal("image/png", metadata.ContentType);
    }

    [Fact]
    public async Task CompleteWithoutPut_ReturnsFailureAndAssetRemainsUploading()
    {
        InitiateUploadResponse initiated = await InitiatePreviewAsync();

        using HttpResponseMessage response = await AppClient.PostAsync($"/files/{initiated.FileId}/complete", null);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(body);

        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("result").ValueKind);
        Assert.True(document.RootElement.GetProperty("errorList").GetArrayLength() > 0);

        MediaAsset asset = await GetAssetAsync(initiated.FileId);
        Assert.Equal(MediaStatus.UPLOADING, asset.Status);
    }

    [Fact]
    public async Task SignedUploadWithOtherContentType_IsRejectedByMinio()
    {
        InitiateUploadResponse initiated = await InitiatePreviewAsync();

        using var request = new HttpRequestMessage(HttpMethod.Put, initiated.Upload.Url)
        {
            Content = new ByteArrayContent(ImageBytes),
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using HttpResponseMessage response = await StorageClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
