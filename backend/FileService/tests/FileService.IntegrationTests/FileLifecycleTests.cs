using System.Net.Http.Json;
using Amazon.S3;
using FileService.Contracts;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;

namespace FileService.IntegrationTests;

public sealed class FileLifecycleTests(FileServiceTestWebFactory factory) : FileServiceIntegrationTestBase(factory)
{
    private static readonly byte[] ImageBytes = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public async Task CancelBeforeDirectPut_MarksAssetDeleted()
    {
        InitiateUploadResponse initiated = await InitiatePreviewAsync();

        using HttpResponseMessage response = await AppClient.PostAsync($"/files/{initiated.FileId}/cancel", null);
        CancelUploadResponse cancelled = await ReadResultAsync<CancelUploadResponse>(response);

        Assert.Equal("deleted", cancelled.Status);
        Assert.Equal(MediaStatus.DELETED, (await GetAssetAsync(initiated.FileId)).Status);
    }

    [Fact]
    public async Task CancelAfterDirectPut_RemovesObjectAndMarksAssetDeleted()
    {
        InitiateUploadResponse initiated = await InitiatePreviewAsync();
        await UploadAsync(initiated.Upload, ImageBytes, "image/png");

        using HttpResponseMessage response = await AppClient.PostAsync($"/files/{initiated.FileId}/cancel", null);
        _ = await ReadResultAsync<CancelUploadResponse>(response);

        var asset = await GetAssetAsync(initiated.FileId);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        await Assert.ThrowsAsync<AmazonS3Exception>(() => Factory.S3Client.GetObjectMetadataAsync(
            asset.RawKey.Bucket,
            asset.RawKey.Value));
    }

    [Fact]
    public async Task DeleteReadyAsset_RemovesStorageObjectAndSoftDeletesAsset()
    {
        InitiateUploadResponse initiated = await InitiatePreviewAsync();
        await UploadAsync(initiated.Upload, ImageBytes, "image/png");
        using (HttpResponseMessage completed = await AppClient.PostAsync($"/files/{initiated.FileId}/complete", null))
            _ = await ReadResultAsync<CompleteUploadResponse>(completed);

        using HttpResponseMessage response = await AppClient.DeleteAsync($"/files/{initiated.FileId}");
        DeleteMediaAssetResponse deleted = await ReadResultAsync<DeleteMediaAssetResponse>(response);
        Assert.Equal("deleted", deleted.Status);

        var asset = await GetAssetAsync(initiated.FileId);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        await Assert.ThrowsAsync<AmazonS3Exception>(() => Factory.S3Client.GetObjectMetadataAsync(
            asset.RawKey.Bucket,
            asset.RawKey.Value));
    }

    [Fact]
    public async Task GetFilesByTarget_ReturnsPendingAndReadyButUrlOnlyForReady()
    {
        Guid targetId = Guid.NewGuid();
        InitiateUploadResponse ready = await InitiatePreviewAsync(targetId);
        InitiateUploadResponse pending = await InitiatePreviewAsync(targetId);
        await UploadAsync(ready.Upload, ImageBytes, "image/png");
        using (HttpResponseMessage completed = await AppClient.PostAsync($"/files/{ready.FileId}/complete", null))
            _ = await ReadResultAsync<CompleteUploadResponse>(completed);

        using HttpResponseMessage response = await AppClient.GetAsync($"/files?targetType=course&targetId={targetId}");
        GetMediaAssetsByTargetResponse files = await ReadResultAsync<GetMediaAssetsByTargetResponse>(response);

        Assert.Equal(2, files.Files.Count);
        Assert.NotNull(files.Files.Single(file => file.FileId == ready.FileId).ContentUrl);
        Assert.Null(files.Files.Single(file => file.FileId == pending.FileId).ContentUrl);
    }
}
