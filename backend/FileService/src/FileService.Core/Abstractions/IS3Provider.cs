using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Contracts;
using FileService.Core.Models;
using FileService.Domain;

namespace FileService.Core;

public interface IS3Provider
{
    Task UploadFileAsync(Stream stream, string bucketName, string key, string contentType, CancellationToken cancellationToken);

    Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
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

    Task<Result<DeleteObjectResponseDto, Error>> DeleteObjectAsync(
        StorageKey storageKey,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken);
}