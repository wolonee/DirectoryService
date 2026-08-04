using FileService.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class S3BucketInitializationService : BackgroundService
{
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<S3BucketInitializationService> _logger;

    public S3BucketInitializationService(
        IOptions<FileStorageOptions> s3Options,
        IServiceScopeFactory scopeFactory,
        ILogger<S3BucketInitializationService> logger)
    {
        _fileStorageOptions = s3Options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (_fileStorageOptions.RequiredBuckets.Count == 0)
            {
                _logger.LogInformation("S3 bucket initialization service required buckets");
                throw new ArgumentException("RequiredBuckets is required");
            }

            _logger.LogInformation(
                "Starting S3 buckets initialization. Required buckets: {Buckets}",
                string.Join(", ", _fileStorageOptions.RequiredBuckets));

            using IServiceScope scope = _scopeFactory.CreateScope();
            IS3Provider s3Provider = scope.ServiceProvider.GetRequiredService<IS3Provider>();

            foreach (string bucketName in _fileStorageOptions.RequiredBuckets)
            {
                await InitializeBucketAsync(s3Provider, bucketName, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("S3 bucket initialization service was cancelled");
        }
        catch (Exception ex)
        {
            var result = S3ErrorMapper.ToError(ex);
            _logger.LogCritical(ex, "Critical error during S3 bucket initialization: {res}", result);
        }
    }
    
    private async Task InitializeBucketAsync(
        IS3Provider s3Provider,
        string bucketName,
        CancellationToken cancellationToken)
    {
        var result = await s3Provider.EnsureBucketExistsAsync(bucketName, cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Could not initialize S3 bucket '{bucketName}': {result.Error.Message}");
        }

        _logger.LogInformation("Bucket {BucketName} is ready", bucketName);
    }
}
