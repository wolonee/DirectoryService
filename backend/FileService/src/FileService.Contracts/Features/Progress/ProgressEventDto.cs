namespace FileService.Contracts.Features.Progress;

public record ProgressEventDto
{
    public Guid MediaAssetId { get; init; }

    public ProcessStatus ProcessStatus { get; init; }

    public double Percent { get; init; }

    public int? StepOrder { get; init; }

    public string? StepName { get; init; }

    public int TotalSteps { get; init; }

    public string? Error { get; init; }

    public string? ErrorCode { get; init; }

    public DateTime PublishedAtUtc { get; init; }
}
