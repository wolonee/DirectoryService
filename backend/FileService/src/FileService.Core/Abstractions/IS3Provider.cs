using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.MultipartUpload.CompleteMultipartUpload;
using FileService.Contracts.Features.MultipartUpload.StartMultipartUpload;
using FileService.Contracts.Features.Simple.InitiateUpload;
using FileService.Contracts.Shared;
using FileService.Core.Models;
using FileService.Domain.S3Entities;

namespace FileService.Core.Abstractions;

public interface IS3Provider
{
    Task<UnitResult<Error>> UploadFileAsync(
        StorageKey storageKey,
        FileStream fileStream,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey storageKey,
        ContentType contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<MultipartPartUploadDto>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> AbortMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        CancellationToken cancellationToken);

    void Dispose();

    Task<Result<PresignedUploadDto, Error>> GenerateUploadUrlAsync(
        StorageKey storageKey,
        ContentType contentType,
        CancellationToken cancellationToken);

    Task<Result<MediaUrl[], Error>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<ObjectMetadataDto, Error>> GetObjectMetadataAsync(
        StorageKey storageKey,
        CancellationToken cancellationToken);

    Task<Result<DeleteObjectResult, Error>> DeleteObjectAsync(
        StorageKey storageKey,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken);
}
