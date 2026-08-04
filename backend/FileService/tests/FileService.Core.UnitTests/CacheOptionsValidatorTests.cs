using FileService.Core.Options;
using FileService.Infrastructure.S3;
using Microsoft.Extensions.Options;
using Xunit;

namespace FileService.Core.UnitTests;

public sealed class CacheOptionsValidatorTests
{
    // Cross-check reads FileStorageOptions.DownloadUrlExpiration; fully-qualify Options.Create
    // to avoid clashing with the FileService.Core.Options namespace.
    private static CacheOptionsValidator CreateValidator(TimeSpan downloadExpiration) =>
        new(Microsoft.Extensions.Options.Options.Create(
            new FileStorageOptions { DownloadUrlExpiration = downloadExpiration }));

    [Fact]
    public void Validate_TtlEqualToDownloadExpiration_Fails()
    {
        CacheOptionsValidator validator = CreateValidator(TimeSpan.FromHours(1));

        ValidateOptionsResult result = validator.Validate(null, new CacheOptions
        {
            RedisEndpoint = "localhost:6379",
            PresignedUrlTtl = TimeSpan.FromHours(1),
            LocalCacheTtl = TimeSpan.FromMinutes(1),
        });

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_NegativeTtl_Fails()
    {
        CacheOptionsValidator validator = CreateValidator(TimeSpan.FromMinutes(10));

        ValidateOptionsResult result = validator.Validate(null, new CacheOptions
        {
            RedisEndpoint = "localhost:6379",
            PresignedUrlTtl = TimeSpan.FromMinutes(-20),
            LocalCacheTtl = TimeSpan.FromMinutes(1),
        });

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_TtlBelowDownloadExpiration_Succeeds()
    {
        CacheOptionsValidator validator = CreateValidator(TimeSpan.FromHours(1));

        ValidateOptionsResult result = validator.Validate(null, new CacheOptions
        {
            RedisEndpoint = "localhost:6379",
            PresignedUrlTtl = TimeSpan.FromMinutes(40),
            LocalCacheTtl = TimeSpan.FromMinutes(5),
        });

        Assert.True(result.Succeeded);
    }
}
