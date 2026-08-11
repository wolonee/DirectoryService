namespace FileService.Domain.S3Entities;

public enum MediaStatus
{
    UPLOADING,
    UPLOADED,
    READY,
    FAILED,
    DELETED,
}