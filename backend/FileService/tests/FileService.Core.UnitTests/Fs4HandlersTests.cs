using System.Collections;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core;
using FileService.Core.Abstractions;
using FileService.Core.Features.SimpleUpload;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileService.Core.UnitTests;

public sealed class Fs4HandlersTests
{
    [Fact]
    public async Task CancelUpload_UploadingAsset_DeletesRawObjectAndMarksAssetDeleted()
    {
        PreviewAsset asset = CreatePreview();
        var repository = new FakeRepository(asset);
        var provider = new FakeS3Provider();
        var handler = new CancelUploadHandler(repository, provider, NullLogger<CancelUploadHandler>.Instance);

        Result<CancelUploadResponse, Error> result = await handler.Handle(new CancelUploadCommand(asset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        Assert.Equal(asset.RawKey, Assert.Single(provider.DeletedKeys));
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task CancelUpload_MissingStorageObject_StillMarksAssetDeleted()
    {
        PreviewAsset asset = CreatePreview();
        var repository = new FakeRepository(asset);
        var handler = new CancelUploadHandler(
            repository,
            new FakeS3Provider(),
            NullLogger<CancelUploadHandler>.Instance);

        Result<CancelUploadResponse, Error> result = await handler.Handle(new CancelUploadCommand(asset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task CancelUpload_FailedAsset_ReturnsInvalidStatus()
    {
        PreviewAsset asset = CreatePreview();
        asset.MarkFailed(DateTime.UtcNow);
        var handler = new CancelUploadHandler(
            new FakeRepository(asset),
            new FakeS3Provider(),
            NullLogger<CancelUploadHandler>.Instance);

        Result<CancelUploadResponse, Error> result = await handler.Handle(new CancelUploadCommand(asset.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("media-asset.invalid-status", result.Error.Code);
    }

    [Fact]
    public async Task Delete_ReadyPreview_DeletesSharedRawAndFinalKeyOnce()
    {
        PreviewAsset asset = CreateReadyPreview();
        var repository = new FakeRepository(asset);
        var provider = new FakeS3Provider();
        var handler = new DeleteMediaAssetHandler(repository, provider, NullLogger<DeleteMediaAssetHandler>.Instance);

        Result<DeleteMediaAssetResponse, Error> result = await handler.Handle(new DeleteFileCommand(asset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        Assert.Equal(asset.FinalKey, Assert.Single(provider.DeletedKeys));
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task Delete_ReadyVideo_DeletesBothDistinctRawAndFinalKeys()
    {
        VideoAsset asset = CreateReadyVideo();
        var repository = new FakeRepository(asset);
        var provider = new FakeS3Provider();
        var handler = new DeleteMediaAssetHandler(repository, provider, NullLogger<DeleteMediaAssetHandler>.Instance);

        Result<DeleteMediaAssetResponse, Error> result = await handler.Handle(new DeleteFileCommand(asset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        Assert.Contains(asset.RawKey, provider.DeletedKeys);
        Assert.Contains(asset.FinalKey, provider.DeletedKeys);
        Assert.Equal(2, provider.DeletedKeys.Count);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task Delete_MissingStorageObject_StillMarksAssetDeleted()
    {
        PreviewAsset asset = CreateReadyPreview();
        var repository = new FakeRepository(asset);
        var handler = new DeleteMediaAssetHandler(
            repository,
            new FakeS3Provider(),
            NullLogger<DeleteMediaAssetHandler>.Instance);

        Result<DeleteMediaAssetResponse, Error> result = await handler.Handle(new DeleteFileCommand(asset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task GetById_GeneratesUrlOnlyForReadyAsset()
    {
        PreviewAsset ready = CreateReadyPreview();
        var provider = new FakeS3Provider();
        var handler = new GetMediaAssetHandler(new FakeRepository(ready), provider);

        Result<GetMediaAssetResponse, Error> result = await handler.Handle(new GetMediaAssetQuery(ready.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("http://minio.test/download", result.Value.ContentUrl);
        Assert.Equal(1, provider.SingleDownloadUrlCalls);
    }

    [Fact]
    public async Task GetByTarget_ReturnsActiveAssetsAndGeneratesUrlsOnlyForReadyAssets()
    {
        Guid targetId = Guid.CreateVersion7();
        PreviewAsset ready = CreateReadyPreview(targetId);
        PreviewAsset uploading = CreatePreview(targetId);
        PreviewAsset deleted = CreatePreview(targetId);
        deleted.MarkDeleted(DateTime.UtcNow);
        var provider = new FakeS3Provider();
        var handler = new GetMediaAssetsByTargetHandler(
            provider,
            new FakeReadDbContext([ready, uploading, deleted]));

        Result<GetMediaAssetsByTargetResponse, Error> result = await handler.Handle(
            new GetMediaAssetsByTargetQuery(new GetMediaAssetsByTargetRequest
            {
                TargetId = targetId,
                TargetType = "course",
            }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Files.Count);
        Assert.Equal(1, provider.BatchDownloadUrlKeys.Count);
        Assert.Contains(result.Value.Files, file => file.FileId == ready.Id && file.ContentUrl is not null);
        Assert.Contains(result.Value.Files, file => file.FileId == uploading.Id && file.ContentUrl is null);
    }

    private static PreviewAsset CreatePreview(Guid? targetId = null) =>
        PreviewAsset.CreateForUpload(
            Guid.CreateVersion7(),
            MediaData.Create(FileName.Create("cover.png").Value, ContentType.Create("image/png").Value, 1024).Value,
            MediaUsage.COURSE_COVER,
            MediaOwner.ForCourse(targetId ?? Guid.CreateVersion7(), Guid.CreateVersion7()).Value).Value;

    private static PreviewAsset CreateReadyPreview(Guid? targetId = null)
    {
        PreviewAsset asset = CreatePreview(targetId);
        StorageReference reference = StorageReference.Create(
            asset.RawKey,
            1024,
            "image/png",
            "etag",
            null,
            DateTime.UtcNow).Value;
        asset.CompleteUpload(reference, DateTime.UtcNow);
        return asset;
    }

    private static VideoAsset CreateReadyVideo()
    {
        VideoAsset asset = VideoAsset.CreateForUpload(
            Guid.CreateVersion7(),
            MediaData.Create(FileName.Create("lesson.mp4").Value, ContentType.Create("video/mp4").Value, 1024).Value,
            MediaUsage.LESSON_VIDEO,
            MediaOwner.ForLesson(Guid.CreateVersion7(), Guid.CreateVersion7()).Value).Value;
        StorageReference reference = StorageReference.Create(
            asset.RawKey,
            1024,
            "video/mp4",
            "etag",
            null,
            DateTime.UtcNow).Value;

        asset.MarkUploaded(DateTime.UtcNow);
        asset.CompleteProcessing(reference, DateTime.UtcNow);

        return asset;
    }

    private sealed class FakeRepository(MediaAsset? asset) : IMediaAssetRepository
    {
        public bool SaveChangesCalled { get; private set; }

        public Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken) =>
            Task.FromResult<Result<Guid, Error>>(mediaAsset.Id);

        public Task<Result<MediaAsset, Error>> GetByIdAsync(Guid fileId, CancellationToken cancellationToken) =>
            Task.FromResult<Result<MediaAsset, Error>>(
                asset?.Id == fileId ? asset : GeneralErrors.NotFound(fileId, "Asset"));

        public Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalled = true;
            return Task.FromResult(UnitResult.Success<Error>());
        }
    }

    private sealed class FakeReadDbContext(IEnumerable<MediaAsset> assets) : IReadDbContext
    {
        public IQueryable<MediaAsset> MediaAssetsQuery => new TestAsyncEnumerable<MediaAsset>(assets);
    }

    private sealed class FakeS3Provider : IS3Provider
    {
        public List<StorageKey> DeletedKeys { get; } = [];

        public List<StorageKey> BatchDownloadUrlKeys { get; } = [];

        public int SingleDownloadUrlCalls { get; private set; }

        public Task<Result<MediaUrl[], Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys, CancellationToken cancellationToken)
        {
            StorageKey[] keys = storageKeys.ToArray();
            BatchDownloadUrlKeys.AddRange(keys);
            return Task.FromResult<Result<MediaUrl[], Error>>(keys.Select(key => new MediaUrl(key, "http://minio.test/download")).ToArray());
        }

        public Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey)
        {
            SingleDownloadUrlCalls++;
            return Task.FromResult<Result<string, Error>>("http://minio.test/download");
        }

        public Task<Result<DeleteObjectResponseDto, Error>> DeleteObjectAsync(StorageKey storageKey, CancellationToken cancellationToken)
        {
            DeletedKeys.Add(storageKey);
            return Task.FromResult<Result<DeleteObjectResponseDto, Error>>(new DeleteObjectResponseDto(null, null));
        }

        public Task<Result<PresignedUploadDto, Error>> GenerateUploadUrlAsync(StorageKey storageKey, ContentType contentType, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<ObjectMetadataDto, Error>> GetObjectMetadataAsync(StorageKey storageKey, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task UploadFileAsync(Stream stream, string bucketName, string key, string contentType, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<string, Error>> StartMultipartUploadAsync(StorageKey storageKey, ContentType contentType, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<IReadOnlyList<MultipartPartUploadDto>, Error>> GenerateAllChunksUploadUrlsAsync(StorageKey storageKey, string uploadId, int totalChunks, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<string, Error>> CompleteMultipartUploadAsync(StorageKey storageKey, string uploadId, IReadOnlyList<PartETagDto> partETags, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey storageKey, string uploadId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<UnitResult<Error>> EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
    {
        public T Current => enumerator.Current;

        public ValueTask DisposeAsync()
        {
            enumerator.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(enumerator.MoveNext());
    }

    private sealed class TestAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<T>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) => inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default) => Execute<TResult>(expression);
    }
}
