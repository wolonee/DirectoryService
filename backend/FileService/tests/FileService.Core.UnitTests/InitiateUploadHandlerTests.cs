using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Abstractions;
using FileService.Core.Features.SimpleUpload;
using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileService.Core.UnitTests;

public sealed class InitiateUploadHandlerTests
{
    [Fact]
    public async Task Handle_ValidPreview_CreatesAssetAndReturnsPresignedUpload()
    {
        var repository = new FakeRepository();
        var provider = new FakeS3Provider();
        var handler = CreateHandler(repository, provider, Guid.CreateVersion7());

        var result = await handler.Handle(
            new InitiateUploadCommand(new InitiateUploadRequest
            {
                FileName = "cover.webp",
                ContentType = "image/webp",
                Size = 1_024,
                AssetType = "preview",
                Usage = "course_cover",
                TargetType = "course",
                TargetId = Guid.CreateVersion7(),
            }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("PUT", result.Value.Upload.Method);
        Assert.Equal("image/webp", result.Value.Upload.RequiredHeaders["Content-Type"]);
        Assert.NotNull(repository.Asset);
        Assert.Equal(MediaStatus.UPLOADING, repository.Asset!.Status);
        Assert.Equal(AssetType.PREVIEW, repository.Asset.AssetType);
        Assert.Equal(repository.Asset.RawKey, provider.RequestedKey);
    }

    [Fact]
    public async Task Handle_UnknownAssetType_ReturnsFailureWithoutCreatingAsset()
    {
        var repository = new FakeRepository();
        var provider = new FakeS3Provider();
        var handler = CreateHandler(repository, provider, Guid.CreateVersion7());

        var result = await handler.Handle(
            new InitiateUploadCommand(new InitiateUploadRequest
            {
                FileName = "cover.webp",
                ContentType = "image/webp",
                Size = 1_024,
                AssetType = "unknown",
                Usage = "course_cover",
                TargetType = "course",
                TargetId = Guid.CreateVersion7(),
            }),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Null(repository.Asset);
        Assert.Null(provider.RequestedKey);
    }

    private static InitiateUploadHandler CreateHandler(
        FakeRepository repository,
        FakeS3Provider provider,
        Guid userId) =>
        new(
            repository,
            new MediaAssetFactory(),
            provider,
            new FakeCurrentUser(userId),
            NullLogger<InitiateUploadHandler>.Instance);

    private sealed class FakeCurrentUser(Guid userId) : ICurrentUser
    {
        public Guid UserId => userId;
    }

    private sealed class FakeRepository : IMediaAssetRepository
    {
        public MediaAsset? Asset { get; private set; }

        public Task<Result<Guid, Error>> AddAsync(MediaAsset asset, CancellationToken cancellationToken)
        {
            Asset = asset;
            return Task.FromResult<Result<Guid, Error>>(asset.Id);
        }

        public Task<Result<MediaAsset, Error>> GetByIdAsync(Guid fileId, CancellationToken cancellationToken) =>
            Task.FromResult<Result<MediaAsset, Error>>(
                Asset?.Id == fileId
                    ? Asset
                    : GeneralErrors.NotFound(fileId, "Asset"));

        public Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken) =>
            Task.FromResult(UnitResult.Success<Error>());
    }

    private sealed class FakeS3Provider : IS3Provider
    {
        public StorageKey? RequestedKey { get; private set; }

        public Task<Result<PresignedUploadDto, Error>> GenerateUploadUrlAsync(
            StorageKey storageKey,
            ContentType contentType,
            CancellationToken cancellationToken)
        {
            RequestedKey = storageKey;
            return Task.FromResult<Result<PresignedUploadDto, Error>>(
                new PresignedUploadDto
                {
                    Url = "http://minio.test/upload",
                    Method = "PUT",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    RequiredHeaders = new Dictionary<string, string>
                    {
                        ["Content-Type"] = contentType.Value,
                    },
                });
        }

        public Task UploadFileAsync(Stream stream, string bucketName, string key, string contentType, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<Result<string, Error>> StartMultipartUploadAsync(string bucketName, string key, string contentType, CancellationToken cancellationToken) =>
            Task.FromResult<Result<string, Error>>("upload-id");

        public Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(string bucketName, string key, string uploadId, int totalChunks, CancellationToken cancellationToken) =>
            Task.FromResult<Result<IReadOnlyList<string>, Error>>(Array.Empty<string>());

        public Task<Result<string, Error>> CompleteMultipartUploadAsync(string bucketName, string key, string uploadId, IReadOnlyList<PartETagDto> partETags, CancellationToken cancellationToken) =>
            Task.FromResult<Result<string, Error>>(key);

        public Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey) =>
            Task.FromResult<Result<string, Error>>("http://minio.test/download");

        public Task<Result<ObjectMetadataDto, Error>> GetObjectMetadataAsync(StorageKey storageKey, CancellationToken cancellationToken) =>
            Task.FromResult<Result<ObjectMetadataDto, Error>>(new ObjectMetadataDto(1, "image/webp", "etag", null, DateTime.UtcNow));

        public Task<Result<DeleteObjectResponseDto, Error>> DeleteObjectAsync(StorageKey storageKey, CancellationToken cancellationToken) =>
            Task.FromResult<Result<DeleteObjectResponseDto, Error>>(new DeleteObjectResponseDto(null, null));

        public Task<UnitResult<Error>> EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken) =>
            Task.FromResult(UnitResult.Success<Error>());

        public void Dispose()
        {
        }
    }
}
