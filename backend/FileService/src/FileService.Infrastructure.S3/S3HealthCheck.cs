using Amazon.S3;
using Amazon.S3.Util;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class S3HealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _s3Options;
    private readonly ILogger<S3HealthCheck> _logger;

    public S3HealthCheck(
        IAmazonS3 s3Client,
        IOptions<S3Options> s3Options,
        ILogger<S3HealthCheck> logger)
    {
        _s3Client = s3Client;
        _s3Options = s3Options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        var firstBucket = _s3Options.RequiredBuckets
            .Select(async bucket =>
            {
                bool result = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucket);
                if (!result)
                {
                    _logger.LogError("Bucket not found");
                    return false;
                }
                
                return true;
            });

        if (!firstBucket.Any())
        {
            _logger.LogError("Bucket not found");
            return HealthCheckResult.Unhealthy();
        }
                
        _logger.LogInformation("Health check completed");
        return HealthCheckResult.Healthy();
    }
}