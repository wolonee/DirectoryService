using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public sealed class S3OptionsValidator : IValidateOptions<FileStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, FileStorageOptions options)
    {
        List<string> failures = [];

        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out Uri? endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add("FileStorageOptions:Endpoint must be an absolute HTTP or HTTPS URL.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey))
            failures.Add("FileStorageOptions:AccessKey must not be empty.");

        if (string.IsNullOrWhiteSpace(options.SecretKey))
            failures.Add("FileStorageOptions:SecretKey must not be empty.");

        if (options.UploadUrlExpiration <= TimeSpan.Zero)
            failures.Add("FileStorageOptions:UploadUrlExpiration must be greater than zero.");

        if (options.DownloadUrlExpiration <= TimeSpan.Zero)
            failures.Add("FileStorageOptions:DownloadUrlExpiration must be greater than zero.");

        if (options.MaxConcurrentRequests <= 0)
            failures.Add("FileStorageOptions:MaxConcurrentRequests must be greater than zero.");

        if (options.MinimumChunkSizeBytes < FileStorageOptions.S3MinimumPartSizeBytes)
            failures.Add($"FileStorageOptions:MinimumChunkSizeBytes must be at least {FileStorageOptions.S3MinimumPartSizeBytes} bytes.");

        if (options.RecommendedChunkSizeBytes < options.MinimumChunkSizeBytes)
            failures.Add("FileStorageOptions:RecommendedChunkSizeBytes must be greater than or equal to MinimumChunkSizeBytes.");

        if (options.MaxChunks is <= 0 or > FileStorageOptions.S3MaximumPartsCount)
            failures.Add($"FileStorageOptions:MaxChunks must be between 1 and {FileStorageOptions.S3MaximumPartsCount}.");

        if (options.RequiredBuckets.Count == 0)
        {
            failures.Add("FileStorageOptions:RequiredBuckets must contain at least one bucket.");
        }
        else
        {
            if (options.RequiredBuckets.Any(string.IsNullOrWhiteSpace))
                failures.Add("FileStorageOptions:RequiredBuckets must not contain empty names.");

            if (options.RequiredBuckets.Distinct(StringComparer.OrdinalIgnoreCase).Count()
                != options.RequiredBuckets.Count)
            {
                failures.Add("FileStorageOptions:RequiredBuckets must not contain duplicate names.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
