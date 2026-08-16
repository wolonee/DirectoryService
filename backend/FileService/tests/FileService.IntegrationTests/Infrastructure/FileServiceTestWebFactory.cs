using System.Data.Common;
using Amazon.S3;
using Amazon.S3.Model;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FileService.Core.Abstractions;
using FileService.Core.Options.FileStorageOptions;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.Postgres.Database;
using FileService.Infrastructure.Postgres.Repositories;
using FileService.Infrastructure.S3;
using FileService.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests.Infrastructure;

public sealed class FileServiceTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string MockUserId = "22222222-2222-2222-2222-222222222222";

    private static readonly string[] BucketNames = ["videos", "preview", "documents"];

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("file_service_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly IContainer _minio = new ContainerBuilder("minio/minio:RELEASE.2025-04-22T22-12-26Z")
        .WithPortBinding(9000, true)
        .WithEnvironment("MINIO_ROOT_USER", "test-access-key")
        .WithEnvironment("MINIO_ROOT_PASSWORD", "test-secret-key")
        .WithCommand("server", "/data")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPort(9000)
                .ForPath("/minio/health/live")))
        .Build();

    private readonly SemaphoreSlim _resetLock = new(1, 1);
    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    public IAmazonS3 S3Client { get; private set; } = null!;

    public string MinioEndpoint =>
        $"http://{_minio.Hostname}:{_minio.GetMappedPublicPort(9000)}";

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _minio.StartAsync());

        S3Client = new AmazonS3Client(
            "test-access-key",
            "test-secret-key",
            new AmazonS3Config
            {
                ServiceURL = MinioEndpoint,
                ForcePathStyle = true,
                UseHttp = true,
            });

        foreach (string bucketName in BucketNames)
            await EnsureBucketAsync(bucketName);

        _ = Services;

        _dbConnection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["files"],
            });
    }

    public new async Task DisposeAsync()
    {
        S3Client?.Dispose();
        _resetLock.Dispose();

        if (_dbConnection is not null)
        {
            await _dbConnection.DisposeAsync();
        }

        await _minio.DisposeAsync();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetStateAsync()
    {
        await _resetLock.WaitAsync();

        try
        {
            await _respawner.ResetAsync(_dbConnection);

            foreach (string bucketName in BucketNames)
                await ClearBucketAsync(bucketName);
        }
        finally
        {
            _resetLock.Release();
        }
    }

    public async Task<T> ExecuteInDbAsync<T>(Func<FileServiceDbContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        FileServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        return await action(dbContext);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var values = new Dictionary<string, string?>
            {
                ["ConnectionStrings:FileServiceDb"] = _postgres.GetConnectionString(),
                ["Development:MockUserId"] = MockUserId,
                ["FileStorageOptions:Endpoint"] = MinioEndpoint,
                ["FileStorageOptions:ExternalEndpoint"] = MinioEndpoint,
                ["FileStorageOptions:AccessKey"] = "test-access-key",
                ["FileStorageOptions:SecretKey"] = "test-secret-key",
                ["FileStorageOptions:WithSsl"] = "false",
                ["FileStorageOptions:UploadUrlExpiration"] = "00:10:00",
                ["FileStorageOptions:DownloadUrlExpiration"] = "00:10:00",
                ["FileStorageOptions:MaxConcurrentRequests"] = "4",
                ["FileStorageOptions:UploadDegreeOfParallelism"] = "10",
                ["FileStorageOptions:MinimumChunkSizeBytes"] = "5242880",
                ["FileStorageOptions:RecommendedChunkSizeBytes"] = "5242880",
                ["FileStorageOptions:MaxChunks"] = "10000",
                ["FileStorageOptions:RequiredBuckets:0"] = BucketNames[0],
                ["FileStorageOptions:RequiredBuckets:1"] = BucketNames[1],
                ["FileStorageOptions:RequiredBuckets:2"] = BucketNames[2],
                ["CacheOptions:RedisEndpoint"] = "localhost:6379",
                ["CacheOptions:PresignedUrlTtl"] = "00:05:00",
                ["CacheOptions:LocalCacheTtl"] = "00:01:00",
            };

            configuration.AddInMemoryCollection(values);
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<FileServiceDbContext>();
            services.RemoveAll<IReadDbContext>();
            services.RemoveAll<IMediaAssetRepository>();
            services.AddScoped<FileServiceDbContext>(_ => new FileServiceDbContext(_postgres.GetConnectionString()));
            services.AddScoped<IReadDbContext>(serviceProvider =>
                serviceProvider.GetRequiredService<FileServiceDbContext>());
            services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();

            services.RemoveAll<IOptions<FileStorageOptions>>();
            services.AddSingleton<IOptions<FileStorageOptions>>(Options.Create(new FileStorageOptions
            {
                Endpoint = MinioEndpoint,
                ExternalEndpoint = MinioEndpoint,
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                WithSsl = false,
                UploadUrlExpiration = TimeSpan.FromMinutes(10),
                DownloadUrlExpiration = TimeSpan.FromMinutes(10),
                MaxConcurrentRequests = 4,
                RequiredBuckets = BucketNames,
                MinimumChunkSizeBytes = 5L * 1024 * 1024,
                RecommendedChunkSizeBytes = 5L * 1024 * 1024,
                MaxChunks = 10_000,
            }));

            services.RemoveAll<IAmazonS3>();
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                "test-access-key",
                "test-secret-key",
                new AmazonS3Config
                {
                    ServiceURL = MinioEndpoint,
                    ForcePathStyle = true,
                    UseHttp = true,
                }));
        });
    }

    private async Task EnsureBucketAsync(string bucketName)
    {
        if (await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(S3Client, bucketName))
            return;

        await S3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });
    }

    private async Task ClearBucketAsync(string bucketName)
    {
        string? keyMarker = null;
        string? uploadIdMarker = null;

        do
        {
            ListMultipartUploadsResponse uploads = await S3Client.ListMultipartUploadsAsync(
                new ListMultipartUploadsRequest
                {
                    BucketName = bucketName,
                    KeyMarker = keyMarker,
                    UploadIdMarker = uploadIdMarker,
                });

            foreach (MultipartUpload upload in uploads.MultipartUploads ?? [])
            {
                await S3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = upload.Key,
                    UploadId = upload.UploadId,
                });
            }

            keyMarker = uploads.NextKeyMarker;
            uploadIdMarker = uploads.NextUploadIdMarker;
        }
        while (!string.IsNullOrWhiteSpace(keyMarker));

        string? continuationToken = null;
        do
        {
            ListObjectsV2Response objects = await S3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucketName,
                ContinuationToken = continuationToken,
            });

            foreach (S3Object item in objects.S3Objects ?? [])
            {
                await S3Client.DeleteObjectAsync(bucketName, item.Key);
            }

            continuationToken = objects.NextContinuationToken;
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));
    }
}
