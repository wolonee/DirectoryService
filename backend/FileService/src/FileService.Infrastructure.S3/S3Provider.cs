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
    private readonly FileStorageOptions _fileStorageOptions;

    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(
        IAmazonS3 s3Client,
        IOptions<FileStorageOptions> s3Options,
        ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _fileStorageOptions = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(_fileStorageOptions.MaxConcurrentRequests);
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
        StorageKey storageKey,
        ContentType contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest()
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                ContentType = contentType.Value,
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

    public async Task<Result<IReadOnlyList<MultipartPartUploadDto>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<MultipartPartUploadDto>> tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);

                    try
                    {
                        var request = new GetPreSignedUrlRequest
                        {
                            BucketName = storageKey.Bucket,
                            Key = storageKey.Value,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.Add(_fileStorageOptions.UploadUrlExpiration),
                            Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                        };

                        string url = await _s3Client.GetPreSignedURLAsync(request)
                            ?? throw new InvalidOperationException("S3 did not return a presigned URL for a multipart part.");

                        return new MultipartPartUploadDto(partNumber, url);
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
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new Amazon.S3.Model.CompleteMultipartUploadRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag
                {
                    ETag = p.ETag,
                    PartNumber = p.PartNumber,
                }).ToList(),
            };

            Amazon.S3.Model.CompleteMultipartUploadResponse response =
                await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error complete multipart upload");
            return S3ErrorMapper.ToError(e);
        }
    }
    
    public async Task<UnitResult<Error>> AbortMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new Amazon.S3.Model.AbortMultipartUploadRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                UploadId = uploadId,
            };
            
            await _s3Client.AbortMultipartUploadAsync(request, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error aborting multipart upload");
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
            DateTime expiresAt = DateTime.UtcNow.Add(_fileStorageOptions.UploadUrlExpiration);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                Verb = HttpVerb.PUT,
                Expires = expiresAt,
                Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
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
                Expires = DateTime.UtcNow.Add(_fileStorageOptions.DownloadUrlExpiration),
                Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
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
                        Expires = DateTime.UtcNow.Add(_fileStorageOptions.DownloadUrlExpiration),
                        Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
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

    public async Task<Result<DeleteObjectResult, Error>> DeleteObjectAsync(
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

            return new DeleteObjectResult(response.DeleteMarker, response.VersionId);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogInformation(
                "S3 object {BucketName}/{ObjectKey} is already absent",
                storageKey.Bucket,
                storageKey.Value);

            return new DeleteObjectResult(null, null);
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
