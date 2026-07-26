using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;


public class S3Provider : IS3Provider, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Provider> _logger;
    private readonly S3Options _s3Options;

    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(
        IAmazonS3 s3Client,
        IOptions<S3Options> s3Options,
        ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(_s3Options.MaxConcurrentRequests);
    }

    public async Task UploadFileAsync(Stream stream, string bucketName, string key, string contentType, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest()
            {
                BucketName = bucketName,
                Key = key,
                ContentType = contentType,
            };

            var response = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

            return response.UploadId;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error starting MultipartUpload");
            return S3ErrorMapper.ToError(e);
        }
    }

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<string>> tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);

                    try
                    {
                        var request = new GetPreSignedUrlRequest
                        {
                            BucketName = bucketName,
                            Key = key,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                        };

                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            return await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error generating upload urls");
            return S3ErrorMapper.ToError(e);
        }
    }

    public async Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag
                {
                    ETag = p.ETag,
                    PartNumber = p.PartNumber,
                }).ToList(),
            };

            CompleteMultipartUploadResponse response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error complete multipart upload");
            return S3ErrorMapper.ToError(e);
        }
    }

    public void Dispose()
    {
        _requestsSemaphore.Dispose();
    }

    public async Task<Result<PresignedUploadDto, Error>> GenerateUploadUrlAsync(
        StorageKey storageKey, 
        ContentType contentType, 
        CancellationToken cancellationToken)
    {
        try
        {
            DateTime expiresAt = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                Verb = HttpVerb.PUT,
                Expires = expiresAt,
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                ContentType = contentType.Value,
            };

            string response = await _s3Client.GetPreSignedURLAsync(request);

            return new PresignedUploadDto
            {
                Url = response,
                Method = "PUT",
                ExpiresAt = expiresAt,
                RequiredHeaders = new Dictionary<string, string>
                {
                    ["Content-Type"] = contentType.Value,
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not generate upload URL for S3 object {BucketName}/{ObjectKey}",
                storageKey.Bucket,
                storageKey.Value);

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                Verb = HttpVerb.GET,
                Expires = DateTime.Now.AddHours(_s3Options.DownloadUrlExpirationHours),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
            };

            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not generate download URL for S3 object {BucketName}/{ObjectKey}",
                storageKey.Bucket,
                storageKey.Value);

            return S3ErrorMapper.ToError(ex);
        }
    }
    
    public async Task<Result<MediaUrl[], Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = storageKeys.Select(async storageKey =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);

                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = storageKey.Bucket,
                        Key = storageKey.Value,
                        Verb = HttpVerb.GET,
                        Expires = DateTime.Now.AddHours(_s3Options.DownloadUrlExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };

                    string? response = await _s3Client.GetPreSignedURLAsync(request);

                    return new MediaUrl(storageKey, response);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });
            
            MediaUrl[] response = await Task.WhenAll(tasks);

            return response;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    
    }

    public async Task<Result<ObjectMetadataDto, Error>> GetObjectMetadataAsync(
        StorageKey storageKey, 
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
            };

            GetObjectMetadataResponse response = await _s3Client.GetObjectMetadataAsync(
                request,
                cancellationToken);

            return new ObjectMetadataDto(
                response.Headers.ContentLength,
                response.ContentType,
                response.ETag,
                response.ChecksumSHA256,
                response.LastModified);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not get metadata for S3 object {BucketName}/{ObjectKey}",
                storageKey.Bucket,
                storageKey.Value);

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<DeleteObjectResponseDto, Error>> DeleteObjectAsync(
        StorageKey storageKey, 
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
            };

            DeleteObjectResponse response = await _s3Client.DeleteObjectAsync(request, cancellationToken);

            return new DeleteObjectResponseDto(response.DeleteMarker, response.VersionId);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogInformation(
                "S3 object {BucketName}/{ObjectKey} is already absent",
                storageKey.Bucket,
                storageKey.Value);

            return new DeleteObjectResponseDto(null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not delete S3 object {BucketName}/{ObjectKey}",
                storageKey.Bucket,
                storageKey.Value);

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> EnsureBucketExistsAsync(
        string bucketName, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            return FileErrors.ValidationFailed("Имя bucket-а не должно быть пустым");

        try
        {
            bool bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (bucketExists)
                return UnitResult.Success<Error>();

            var request = new PutBucketRequest
            {
                BucketName = bucketName,
            };

            await _s3Client.PutBucketAsync(request, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou")
        {
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not ensure that S3 bucket {BucketName} exists", bucketName);
            return S3ErrorMapper.ToError(ex);
        }
    }

}
