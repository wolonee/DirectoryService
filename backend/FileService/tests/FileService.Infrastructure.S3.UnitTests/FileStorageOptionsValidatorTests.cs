using FileService.Core.Options.FileStorageOptions;
using FileService.Infrastructure.S3;

namespace FileService.Infrastructure.S3.UnitTests;

public class FileStorageOptionsValidatorTests
{
    private readonly FileStorageOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenOptionsAreValid_ShouldSucceed()
    {
        // Act
        var result = _validator.Validate(null, CreateValidOptions());

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenUploadUrlExpirationIsNotPositive_ShouldFail(int expirationHours)
    {
        // Arrange
        FileStorageOptions options = CreateValidOptions() with
        {
            UploadUrlExpiration = TimeSpan.FromHours(expirationHours),
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failures);
        Assert.Contains("FileStorageOptions:UploadUrlExpiration must be greater than zero.", result.Failures);
    }

    [Fact]
    public void Validate_WhenRequiredBucketsAreEmpty_ShouldFail()
    {
        // Arrange
        FileStorageOptions options = CreateValidOptions() with
        {
            RequiredBuckets = [],
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failures);
        Assert.Contains("FileStorageOptions:RequiredBuckets must contain at least one bucket.", result.Failures);
    }

    [Fact]
    public void Validate_WhenMaxChunksExceedsS3Limit_ShouldFail()
    {
        // Arrange
        FileStorageOptions options = CreateValidOptions() with
        {
            MaxChunks = FileStorageOptions.S3MaximumPartsCount + 1,
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("FileStorageOptions:MaxChunks must be between 1 and 10000.", result.Failures);
    }

    [Fact]
    public void Validate_WhenMinimumChunkSizeIsLessThanS3Limit_ShouldFail()
    {
        // Arrange
        FileStorageOptions options = CreateValidOptions() with
        {
            MinimumChunkSizeBytes = FileStorageOptions.S3MinimumPartSizeBytes - 1,
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(
            $"FileStorageOptions:MinimumChunkSizeBytes must be at least {FileStorageOptions.S3MinimumPartSizeBytes} bytes.",
            result.Failures);
    }

    private static FileStorageOptions CreateValidOptions() => new()
    {
        Endpoint = "http://minio:9000",
        AccessKey = "test-access-key",
        SecretKey = "test-secret-key",
        UploadUrlExpiration = TimeSpan.FromHours(1),
        DownloadUrlExpiration = TimeSpan.FromHours(1),
        MaxConcurrentRequests = 1,
        RequiredBuckets = ["preview"],
    };
}
