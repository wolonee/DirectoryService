using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Amazon.S3;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FileService.Domain;
using FileService.Infrastructure.S3;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3.UnitTests;

public class S3ProviderPresignedUploadIntegrationTests : IAsyncLifetime
{
    private const string AccessKey = "test-access-key";
    private const string SecretKey = "test-secret-key";
    private const string BucketName = "preview";

    private readonly IContainer _minioContainer = new ContainerBuilder("minio/minio:RELEASE.2025-04-22T22-12-26Z")
        .WithPortBinding(9000, true)
        .WithEnvironment("MINIO_ROOT_USER", AccessKey)
        .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
        .WithCommand("server", "/data")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPort(9000)
                .ForPath("/minio/health/live")))
        .Build();

    public async Task InitializeAsync()
    {
        await _minioContainer.StartAsync();
    }

    public Task DisposeAsync() => _minioContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task PresignedPutUrl_ShouldUploadReadAndDeleteSmallFileDirectlyInMinio()
    {
        // Arrange
        using var provider = CreateProvider();
        using var httpClient = new HttpClient();

        StorageKey storageKey = StorageKey.Create(
            BucketName,
            "tests",
            $"{Guid.NewGuid():N}.txt").Value;
        ContentType contentType = ContentType.Create("text/plain").Value;
        byte[] expectedContent = Encoding.UTF8.GetBytes("Small file uploaded by a presigned PUT URL.");

        var ensureBucketResult = await provider.EnsureBucketExistsAsync(BucketName, CancellationToken.None);
        Assert.True(ensureBucketResult.IsSuccess);

        try
        {
            // Act
            var uploadUrlResult = await provider.GenerateUploadUrlAsync(
                storageKey,
                contentType,
                CancellationToken.None);

            // Assert: browser/client uploads bytes directly to MinIO, not through File Service.
            Assert.True(uploadUrlResult.IsSuccess);
            Assert.Equal("PUT", uploadUrlResult.Value.Method);
            Assert.Equal(contentType.Value, uploadUrlResult.Value.RequiredHeaders["Content-Type"]);

            using var uploadContent = new ByteArrayContent(expectedContent);
            uploadContent.Headers.ContentType = new MediaTypeHeaderValue(contentType.Value);

            using HttpResponseMessage uploadResponse = await httpClient.PutAsync(
                uploadUrlResult.Value.Url,
                uploadContent,
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

            var metadataResult = await provider.GetObjectMetadataAsync(storageKey, CancellationToken.None);
            Assert.True(metadataResult.IsSuccess);
            Assert.Equal(expectedContent.Length, metadataResult.Value.ContentLength);
            Assert.Equal(contentType.Value, metadataResult.Value.ContentType);
            Assert.False(string.IsNullOrWhiteSpace(metadataResult.Value.ETag));

            var downloadUrlResult = await provider.GenerateDownloadUrlAsync(storageKey);
            Assert.True(downloadUrlResult.IsSuccess);

            byte[] actualContent = await httpClient.GetByteArrayAsync(downloadUrlResult.Value);
            Assert.Equal(expectedContent, actualContent);

            var firstDeleteResult = await provider.DeleteObjectAsync(storageKey, CancellationToken.None);
            var secondDeleteResult = await provider.DeleteObjectAsync(storageKey, CancellationToken.None);

            Assert.True(firstDeleteResult.IsSuccess);
            Assert.True(secondDeleteResult.IsSuccess);
        }
        finally
        {
            await provider.DeleteObjectAsync(storageKey, CancellationToken.None);
        }
    }

    [Fact]
    public async Task PresignedPutUrl_WhenContentTypeDoesNotMatch_ShouldBeRejectedByMinio()
    {
        // Arrange
        using var provider = CreateProvider();
        using var httpClient = new HttpClient();

        StorageKey storageKey = StorageKey.Create(
            BucketName,
            "tests",
            $"{Guid.NewGuid():N}.txt").Value;
        ContentType signedContentType = ContentType.Create("text/plain").Value;

        var ensureBucketResult = await provider.EnsureBucketExistsAsync(BucketName, CancellationToken.None);
        Assert.True(ensureBucketResult.IsSuccess);

        try
        {
            var uploadUrlResult = await provider.GenerateUploadUrlAsync(
                storageKey,
                signedContentType,
                CancellationToken.None);
            Assert.True(uploadUrlResult.IsSuccess);

            using var uploadContent = new ByteArrayContent(Encoding.UTF8.GetBytes("wrong content type"));
            uploadContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            using HttpResponseMessage uploadResponse = await httpClient.PutAsync(
                uploadUrlResult.Value.Url,
                uploadContent,
                CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, uploadResponse.StatusCode);
        }
        finally
        {
            await provider.DeleteObjectAsync(storageKey, CancellationToken.None);
        }
    }

    private S3Provider CreateProvider()
    {
        var client = new AmazonS3Client(
            AccessKey,
            SecretKey,
            new AmazonS3Config
            {
                ServiceURL = $"http://{_minioContainer.Hostname}:{_minioContainer.GetMappedPublicPort(9000)}",
                ForcePathStyle = true,
            });

        return new S3Provider(
            client,
            Options.Create(new S3Options
            {
                UploadUrlExpirationHours = 1,
                DownloadUrlExpirationHours = 1,
                MaxConcurrentRequests = 1,
            }),
            NullLogger<S3Provider>.Instance);
    }
}
