using FileService.Infrastructure.S3;
using Microsoft.Extensions.Options;

namespace FileService.Core.Options;

public sealed class CacheOptionsValidator : IValidateOptions<CacheOptions>
{
    private readonly FileStorageOptions _fileStorageOptions;

    public CacheOptionsValidator(IOptions<FileStorageOptions> fileStorageOptions)
    {
        _fileStorageOptions = fileStorageOptions.Value;
    }

    public ValidateOptionsResult Validate(string? name, CacheOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.RedisEndpoint))
            failures.Add("CacheOptions:RedisEndpoint must not be empty.");

        if (options.PresignedUrlTtl <= TimeSpan.Zero)
            failures.Add("CacheOptions:PresignedUrlTtl must be greater than zero.");

        if (options.LocalCacheTtl <= TimeSpan.Zero)
            failures.Add("CacheOptions:LocalCacheTtl must be greater than zero.");

        if (options.LocalCacheTtl > options.PresignedUrlTtl)
            failures.Add("CacheOptions:LocalCacheTtl must not exceed PresignedUrlTtl.");

        if (options.PresignedUrlTtl >= _fileStorageOptions.DownloadUrlExpiration)
        {
            failures.Add(
                $"CacheOptions:PresignedUrlTtl ({options.PresignedUrlTtl}) must be strictly less than "
                + $"FileStorageOptions:DownloadUrlExpiration ({_fileStorageOptions.DownloadUrlExpiration}).");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
