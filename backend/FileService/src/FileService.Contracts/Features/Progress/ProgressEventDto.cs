namespace FileService.Contracts.Features.Progress;

public record ProgressEventDto
{
    public Guid MediaAssetId;
    public ProcessStatus ProcessStatus;
    public double Percent;
    public int? StepOrder;
    public string? StepName;
    public int TotalSteps;
    public string? Error;
    public string? ErrorCode;
    public DateTime PublishedAtUtc;
}