using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public sealed class S3OptionsValidator : IValidateOptions<S3Options>
{
    public ValidateOptionsResult Validate(string? name, S3Options options)
    {
        List<string> failures = [];

        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out Uri? endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add("S3Options:Endpoint must be an absolute HTTP or HTTPS URL.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey))
            failures.Add("S3Options:AccessKey must not be empty.");

        if (string.IsNullOrWhiteSpace(options.SecretKey))
            failures.Add("S3Options:SecretKey must not be empty.");

        if (options.UploadUrlExpiration <= TimeSpan.Zero)
            failures.Add("S3Options:UploadUrlExpiration must be greater than zero.");

        if (options.DownloadUrlExpiration <= TimeSpan.Zero)
            failures.Add("S3Options:DownloadUrlExpiration must be greater than zero.");

        if (options.MaxConcurrentRequests <= 0)
            failures.Add("S3Options:MaxConcurrentRequests must be greater than zero.");

        if (options.MinimumChunkSizeBytes < S3Options.S3MinimumPartSizeBytes)
            failures.Add($"S3Options:MinimumChunkSizeBytes must be at least {S3Options.S3MinimumPartSizeBytes} bytes.");

        if (options.RecommendedChunkSizeBytes < options.MinimumChunkSizeBytes)
            failures.Add("S3Options:RecommendedChunkSizeBytes must be greater than or equal to MinimumChunkSizeBytes.");

        if (options.MaxChunks is <= 0 or > S3Options.S3MaximumPartsCount)
            failures.Add($"S3Options:MaxChunks must be between 1 and {S3Options.S3MaximumPartsCount}.");

        if (options.RequiredBuckets.Count == 0)
        {
            failures.Add("S3Options:RequiredBuckets must contain at least one bucket.");
        }
        else
        {
            if (options.RequiredBuckets.Any(string.IsNullOrWhiteSpace))
                failures.Add("S3Options:RequiredBuckets must not contain empty names.");

            if (options.RequiredBuckets.Distinct(StringComparer.OrdinalIgnoreCase).Count()
                != options.RequiredBuckets.Count)
            {
                failures.Add("S3Options:RequiredBuckets must not contain duplicate names.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
