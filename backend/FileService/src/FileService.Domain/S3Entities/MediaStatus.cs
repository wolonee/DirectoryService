namespace FileService.Domain.S3Entities;

public enum MediaStatus
{
    PROCESSING,
    UPLOADING,
    UPLOADED,
    READY,
    FAILED,
    DELETED,
}