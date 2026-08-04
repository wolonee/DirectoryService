using Amazon.S3;
using Amazon.S3.Util;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class S3HealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3Client;
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly ILogger<S3HealthCheck> _logger;

    public S3HealthCheck(
        IAmazonS3 s3Client,
        IOptions<FileStorageOptions> s3Options,
        ILogger<S3HealthCheck> logger)
    {
        _s3Client = s3Client;
        _fileStorageOptions = s3Options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        if (_fileStorageOptions.RequiredBuckets.Count == 0)
        {
            _logger.LogError("Required S3 buckets are not configured");
            return HealthCheckResult.Unhealthy("Required S3 buckets are not configured");
        }

        try
        {
            foreach (string bucketName in _fileStorageOptions.RequiredBuckets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(
                    _s3Client,
                    bucketName);

                if (!bucketExists)
                {
                    _logger.LogError("Required S3 bucket {BucketName} was not found", bucketName);
                    return HealthCheckResult.Unhealthy(
                        $"Required S3 bucket '{bucketName}' was not found");
                }
            }

            _logger.LogDebug("S3 health check completed successfully");
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "S3 health check failed");
            return HealthCheckResult.Unhealthy("Object storage is unavailable", ex);
        }
    }
}
