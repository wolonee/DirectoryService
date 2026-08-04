using Amazon.S3;
using Amazon.S3.Model;
using FileService.Domain;
using FileService.Infrastructure.S3;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3.UnitTests;

public class S3ProviderTests
{
    [Fact]
    public async Task DeleteObjectAsync_WhenObjectDoesNotExist_ShouldReturnSuccess()
    {
        // Arrange
        using var s3Client = new MissingObjectS3Client();
        using var provider = new S3Provider(
            s3Client,
            Options.Create(new FileStorageOptions
            {
                MaxConcurrentRequests = 1,
            }),
            NullLogger<S3Provider>.Instance);

        StorageKey storageKey = StorageKey.Create(
            "preview",
            "raw",
            "missing-file.png").Value;

        // Act
        var result = await provider.DeleteObjectAsync(storageKey, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.DeleteMarker);
        Assert.Null(result.Value.VersionId);
    }

    private sealed class MissingObjectS3Client : AmazonS3Client
    {
        public MissingObjectS3Client()
            : base(
                "test-access-key",
                "test-secret-key",
                new AmazonS3Config
                {
                    ServiceURL = "http://localhost:9000",
                    ForcePathStyle = true,
                })
        {
        }

        public override Task<DeleteObjectResponse> DeleteObjectAsync(
            DeleteObjectRequest request,
            CancellationToken cancellationToken = default)
        {
            var exception = new AmazonS3Exception("Object does not exist")
            {
                ErrorCode = "NoSuchKey",
            };

            return Task.FromException<DeleteObjectResponse>(exception);
        }
    }
}
