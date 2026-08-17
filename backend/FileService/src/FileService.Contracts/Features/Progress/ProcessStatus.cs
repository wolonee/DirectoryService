namespace FileService.Contracts.Features.Progress;

public enum ProcessStatus
{
    QUEUED,
    PROCESSING,
    READY,
    FAILED,
}