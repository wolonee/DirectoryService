using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core;
using FileService.Core.Abstractions;
using FileService.Core.Features;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Infrastructure.S3;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileService.Core.UnitTests;

public sealed class MultipartUploadHandlersTests
{
    [Fact]
    public async Task Start_ThenComplete_WithSeveralParts_SavesUploadIdAndMarksAssetReady()
    {
        Guid userId = Guid.CreateVersion7();
        var repository = new FakeRepository();
        var provider = new FakeS3Provider();
        var startHandler = new StartMultipartUploadHandler(
            provider,
            repository,
            new MediaAssetFactory(),
            new FakeChunkSizeCalculator(5 * 1024 * 1024, 2),
            new StartMultipartUploadValidator(),
            new FakeCurrentUser(userId),
            NullLogger<StartMultipartUploadHandler>.Instance);

        Result<StartMultipartUploadResponse, Errors> startResult = await startHandler.Handle(
            CreateStartCommand(6 * 1024 * 1024),
            CancellationToken.None);

        Assert.True(startResult.IsSuccess);
        Assert.Equal("multipart-upload-id", repository.Asset!.MultipartUploadId);
        Assert.Equal(1, repository.SaveChangesCalls);

        var completeHandler = new CompleteMultipartUploadHandler(
            provider,
            repository,
            new FakeCurrentUser(userId),
            new CompleteMultipartUploadValidator(),
            NullLogger<CompleteMultipartUploadHandler>.Instance);

        Result<CompleteMultipartUploadResponse, Errors> completeResult = await completeHandler.Handle(
            new CompleteMultipartUploadCommand(new CompleteMultipartUploadRequest
            {
                FileId = startResult.Value.FileId,
                UploadId = startResult.Value.UploadId,
                Parts =
                [
                    new PartETagDto(1, "etag-1"),
                    new PartETagDto(2, "etag-2"),
                ],
            }),
            CancellationToken.None);

        Assert.True(completeResult.IsSuccess);
        Assert.Equal(MediaStatus.READY, repository.Asset.Status);
        Assert.Equal(1, provider.CompleteCalls);
        Assert.NotNull(repository.Asset.StorageReference);
    }

    [Fact]
    public async Task Complete_WithInvalidParts_DoesNotCompleteMultipartUpload()
    {
        Guid userId = Guid.CreateVersion7();
        MediaAsset asset = CreateUploadingAsset(userId, expectedChunksCount: 2);
        asset.SetMultipartUploadId("multipart-upload-id");
        var repository = new FakeRepository(asset);
        var provider = new FakeS3Provider();
        var handler = new CompleteMultipartUploadHandler(
            provider,
            repository,
            new FakeCurrentUser(userId),
            new CompleteMultipartUploadValidator(),
            NullLogger<CompleteMultipartUploadHandler>.Instance);

        Result<CompleteMultipartUploadResponse, Errors> result = await handler.Handle(
            new CompleteMultipartUploadCommand(new CompleteMultipartUploadRequest
            {
                FileId = asset.Id,
                UploadId = "multipart-upload-id",
                Parts =
                [
                    new PartETagDto(1, "etag-1"),
                    new PartETagDto(1, "etag-duplicate"),
                ],
            }),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(0, provider.CompleteCalls);
        Assert.Equal(MediaStatus.UPLOADING, asset.Status);
    }

    [Fact]
    public async Task Complete_WithAnotherUploadId_DoesNotCompleteMultipartUpload()
    {
        Guid userId = Guid.CreateVersion7();
        MediaAsset asset = CreateUploadingAsset(userId, expectedChunksCount: 1);
        asset.SetMultipartUploadId("multipart-upload-id");
        var provider = new FakeS3Provider();
        var handler = new CompleteMultipartUploadHandler(
            provider,
            new FakeRepository(asset),
            new FakeCurrentUser(userId),
            new CompleteMultipartUploadValidator(),
            NullLogger<CompleteMultipartUploadHandler>.Instance);

        Result<CompleteMultipartUploadResponse, Errors> result = await handler.Handle(
            new CompleteMultipartUploadCommand(new CompleteMultipartUploadRequest
            {
                FileId = asset.Id,
                UploadId = "another-upload-id",
                Parts = [new PartETagDto(1, "etag-1")],
            }),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("media-asset.invalid-multipart-upload-id", result.Error.Single().Code);
        Assert.Equal(0, provider.CompleteCalls);
    }

    [Fact]
    public async Task Complete_AfterReady_ReturnsAlreadyCompleted()
    {
        Guid userId = Guid.CreateVersion7();
        MediaAsset asset = CreateUploadingAsset(userId, expectedChunksCount: 1);
        asset.SetMultipartUploadId("multipart-upload-id");
        asset.MarkUploaded(DateTime.UtcNow);
        asset.MarkReady(asset.RawKey, CreateStorageReference(asset), DateTime.UtcNow);
        var provider = new FakeS3Provider();
        var handler = new CompleteMultipartUploadHandler(
            provider,
            new FakeRepository(asset),
            new FakeCurrentUser(userId),
            new CompleteMultipartUploadValidator(),
            NullLogger<CompleteMultipartUploadHandler>.Instance);

        Result<CompleteMultipartUploadResponse, Errors> result = await handler.Handle(
            new CompleteMultipartUploadCommand(new CompleteMultipartUploadRequest
            {
                FileId = asset.Id,
                UploadId = "multipart-upload-id",
                Parts = [new PartETagDto(1, "etag-1")],
            }),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("media-asset.already-completed", result.Error.Single().Code);
        Assert.Equal(0, provider.CompleteCalls);
    }

    [Fact]
    public async Task Abort_PartiallyUploadedAsset_AbortsStorageAndMarksAssetDeleted()
    {
        Guid userId = Guid.CreateVersion7();
        MediaAsset asset = CreateUploadingAsset(userId, expectedChunksCount: 2);
        asset.SetMultipartUploadId("multipart-upload-id");
        var repository = new FakeRepository(asset);
        var provider = new FakeS3Provider();
        var handler = new AbortMultipartUploadHandler(
            provider,
            repository,
            new FakeCurrentUser(userId),
            new AbortMultipartUploadValidator(),
            NullLogger<AbortMultipartUploadHandler>.Instance);

        Result<AbortMultipartUploadResponse, Errors> result = await handler.Handle(
            new AbortMultipartUploadCommand(new AbortMultipartUploadRequest
            {
                FileId = asset.Id,
                UploadId = "multipart-upload-id",
            }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        Assert.Equal(asset.RawKey, provider.AbortedKey);
        Assert.Equal("multipart-upload-id", provider.AbortedUploadId);
        Assert.True(repository.SaveChangesCalls > 0);
    }

    private static StartMultipartUploadCommand CreateStartCommand(long size) =>
        new(new StartMultipartUploadRequest
        {
            FileName = "cover.png",
            ContentType = "image/png",
            Size = size,
            AssetType = "preview",
            Usage = "course_cover",
            TargetType = "course",
            TargetId = Guid.CreateVersion7(),
        });

    private static MediaAsset CreateUploadingAsset(Guid userId, int expectedChunksCount)
    {
        FileName fileName = FileName.Create("cover.png").Value;
        ContentType contentType = ContentType.Create("image/png").Value;
        MediaData mediaData = MediaData.Create(fileName, contentType, 6 * 1024 * 1024, expectedChunksCount).Value;
        MediaOwner owner = MediaOwner.ForCourse(Guid.CreateVersion7(), userId).Value;

        return PreviewAsset.CreateForUpload(
            Guid.CreateVersion7(),
            mediaData,
            MediaUsage.COURSE_COVER,
            owner).Value;
    }

    private static StorageReference CreateStorageReference(MediaAsset asset) =>
        StorageReference.Create(
            asset.RawKey,
            asset.MediaData.Size,
            asset.MediaData.ContentType.Value,
            "etag",
            null,
            DateTime.UtcNow).Value;

    private sealed class FakeCurrentUser(Guid userId) : ICurrentUser
    {
        public Guid UserId => userId;
    }

    private sealed class FakeChunkSizeCalculator(long chunkSize, int totalChunks) : IChunkSizeCalculator
    {
        public Result<(long ChunkSize, int TotalChunks), Error> CalculateChunkSize(long fileSize) =>
            (chunkSize, totalChunks);
    }

    private sealed class FakeRepository(MediaAsset? asset = null) : IMediaAssetRepository
    {
        public MediaAsset? Asset { get; private set; } = asset;

        public int SaveChangesCalls { get; private set; }

        public Task<Result<Guid, Error>> AddAsync(MediaAsset asset, CancellationToken cancellationToken)
        {
            Asset = asset;
            return Task.FromResult<Result<Guid, Error>>(asset.Id);
        }

        public Task<Result<MediaAsset, Error>> GetByIdAsync(Guid fileId, CancellationToken cancellationToken) =>
            Task.FromResult<Result<MediaAsset, Error>>(
                Asset?.Id == fileId ? Asset : GeneralErrors.NotFound(fileId, "Asset"));

        public Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(UnitResult.Success<Error>());
        }
    }

    private sealed class FakeS3Provider : IS3Provider
    {
        public int CompleteCalls { get; private set; }

        public StorageKey? AbortedKey { get; private set; }

        public string? AbortedUploadId { get; private set; }

        public Task<Result<string, Error>> StartMultipartUploadAsync(
            StorageKey storageKey,
            ContentType contentType,
            CancellationToken cancellationToken) =>
            Task.FromResult<Result<string, Error>>("multipart-upload-id");

        public Task<Result<IReadOnlyList<MultipartPartUploadDto>, Error>> GenerateAllChunksUploadUrlsAsync(
            StorageKey storageKey,
            string uploadId,
            int totalChunks,
            CancellationToken cancellationToken) =>
            Task.FromResult<Result<IReadOnlyList<MultipartPartUploadDto>, Error>>(
                Enumerable.Range(1, totalChunks)
                    .Select(number => new MultipartPartUploadDto(number, $"http://minio.test/part/{number}"))
                    .ToArray());

        public Task<Result<string, Error>> CompleteMultipartUploadAsync(
            StorageKey storageKey,
            string uploadId,
            IReadOnlyList<PartETagDto> partETags,
            CancellationToken cancellationToken)
        {
            CompleteCalls++;
            return Task.FromResult<Result<string, Error>>(storageKey.Value);
        }

        public Task<UnitResult<Error>> AbortMultipartUploadAsync(
            StorageKey storageKey,
            string uploadId,
            CancellationToken cancellationToken)
        {
            AbortedKey = storageKey;
            AbortedUploadId = uploadId;
            return Task.FromResult(UnitResult.Success<Error>());
        }

        public Task<Result<ObjectMetadataDto, Error>> GetObjectMetadataAsync(
            StorageKey storageKey,
            CancellationToken cancellationToken) =>
            Task.FromResult<Result<ObjectMetadataDto, Error>>(
                new ObjectMetadataDto(6 * 1024 * 1024, "image/png", "etag", null, DateTime.UtcNow));

        public Task<Result<PresignedUploadDto, Error>> GenerateUploadUrlAsync(StorageKey storageKey, ContentType contentType, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<MediaUrl[], Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey) => throw new NotSupportedException();

        public Task<Result<DeleteObjectResult, Error>> DeleteObjectAsync(StorageKey storageKey, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<UnitResult<Error>> EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task UploadFileAsync(Stream stream, string bucketName, string key, string contentType, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
