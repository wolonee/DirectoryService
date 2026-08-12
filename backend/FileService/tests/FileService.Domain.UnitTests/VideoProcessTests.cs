using System.Reflection;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.Domain.UnitTests;

public class VideoProcessTests
{
    // ---------- Construction ----------

    [Fact]
    public void Constructor_InitializesProcessInProgressWithAllStepsPending()
    {
        Guid videoAssetId = Guid.CreateVersion7();

        VideoProcess process = CreateProcess(videoAssetId);

        Assert.Equal(videoAssetId, process.VideoAssetId);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Null(process.CurrentStep);
        Assert.Null(process.CompletedAt);
        Assert.Equal(6, process.Steps.Count);
        Assert.All(process.Steps, step => Assert.Equal(StepStatus.PENDING, step.Status));
    }

    [Fact]
    public void Constructor_CreatesStepsInWeightedPipelineOrder()
    {
        VideoProcess process = CreateProcess();

        StepType[] orderedTypes = process.Steps
            .OrderBy(s => s.Order)
            .Select(s => s.StepType)
            .ToArray();

        Assert.Equal(
            new[]
            {
                StepType.INITIALIZE,
                StepType.EXTRACT_METADATA,
                StepType.GENERATE_HLS,
                StepType.UPLOAD_HLS,
                StepType.GENERATE_PREVIEW,
                StepType.CLEANUP,
            },
            orderedTypes);

        // Weights must add up to a full 100% run.
        Assert.Equal(100, process.Steps.Sum(s => s.Weight));
    }

    // ---------- ProcessNextStep ----------

    [Fact]
    public void ProcessNextStep_FirstCall_StartsFirstStepInOrder()
    {
        VideoProcess process = CreateProcess();

        var result = process.ProcessNextStep();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StepType.INITIALIZE, result.Value!.StepType);
        Assert.Equal(StepStatus.IN_PROGRESS, result.Value.Status);
        Assert.Same(result.Value, process.CurrentStep);
    }

    [Fact]
    public void ProcessNextStep_WhenStepAlreadyInProgress_ReturnsSameStepWithoutAdvancing()
    {
        VideoProcess process = CreateProcess();
        ProcessingStep? first = process.ProcessNextStep().Value;

        var second = process.ProcessNextStep();

        Assert.True(second.IsSuccess);
        Assert.Same(first, second.Value);
        // Exactly one step may be in progress at any time.
        Assert.Single(process.Steps, s => s.Status == StepStatus.IN_PROGRESS);
    }

    [Fact]
    public void ProcessNextStep_WhenNotInProgress_ReturnsFailure()
    {
        VideoProcess process = CreateProcess();
        process.Fail("aborted");

        var result = process.ProcessNextStep();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    // ---------- CompleteCurrentStep / progress ----------

    [Fact]
    public void CompleteCurrentStep_AccumulatesProgressByCompletedStepWeights()
    {
        VideoProcess process = CreateProcess();

        // INITIALIZE (weight 0)
        process.ProcessNextStep();
        process.CompleteCurrentStep();
        Assert.Equal(0, process.ProgressPercentage);

        // EXTRACT_METADATA (weight 10)
        process.ProcessNextStep();
        var result = process.CompleteCurrentStep("metadata.json");

        Assert.True(result.IsSuccess);
        Assert.Equal(10, process.ProgressPercentage);
        Assert.Null(process.CurrentStep);
    }

    [Fact]
    public void CompleteCurrentStep_WhenNoActiveStep_ReturnsValidationError()
    {
        VideoProcess process = CreateProcess();

        var result = process.CompleteCurrentStep();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.no.active.step", result.Error.Code);
    }

    [Fact]
    public void CompleteCurrentStep_WhenNotInProgress_ReturnsValidationError()
    {
        VideoProcess process = CreateProcess();
        process.Fail("aborted");

        var result = process.CompleteCurrentStep();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    // ---------- Full happy path ----------

    [Fact]
    public void FullPipeline_CompletesAllSteps_ReachesHundredPercentAndCompletedStatus()
    {
        VideoProcess process = CreateProcess();

        // Drive every step: start it, then complete it.
        for (int i = 0; i < process.Steps.Count; i++)
        {
            var startResult = process.ProcessNextStep();
            Assert.True(startResult.IsSuccess);
            Assert.NotNull(startResult.Value);

            var completeResult = process.CompleteCurrentStep($"result-{i}");
            Assert.True(completeResult.IsSuccess);
        }

        // One more call: nothing pending -> the process finalizes itself.
        var finalize = process.ProcessNextStep();

        Assert.True(finalize.IsSuccess);
        Assert.Null(finalize.Value);
        Assert.Equal(ProcessingStatus.COMPLETED, process.Status);
        Assert.Equal(100, process.ProgressPercentage);
        Assert.NotNull(process.CompletedAt);
        Assert.All(process.Steps, s => Assert.Equal(StepStatus.COMPLETED, s.Status));
    }

    // ---------- FailCurrentStep ----------

    [Fact]
    public void FailCurrentStep_MarksActiveStepFailed_ButKeepsProcessInProgress()
    {
        VideoProcess process = CreateProcess();
        ProcessingStep? step = process.ProcessNextStep().Value;

        var result = process.FailCurrentStep("ffmpeg crashed");

        Assert.True(result.IsSuccess);
        Assert.Equal(StepStatus.FAILED, step!.Status);
        Assert.Equal("ffmpeg crashed", step.ErrorMessage);
        // Failing a single step does not fail the whole process.
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
    }

    [Fact]
    public void FailCurrentStep_WhenNoActiveStep_ReturnsValidationError()
    {
        VideoProcess process = CreateProcess();

        var result = process.FailCurrentStep("boom");

        Assert.True(result.IsFailure);
        Assert.Equal("processing.no.active.step", result.Error.Code);
    }

    // ---------- Fail (whole process) ----------

    [Fact]
    public void Fail_FromInProgress_SetsFailedStatusErrorAndCompletedAt()
    {
        VideoProcess process = CreateProcess();

        var result = process.Fail("disk full", isCritical: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.FAILED, process.Status);
        Assert.Equal("disk full", process.ErrorMessage);
        Assert.True(process.IsCriticalError);
        Assert.NotNull(process.CompletedAt);
    }

    [Fact]
    public void Fail_WithEmptyMessage_ReturnsValidationError()
    {
        VideoProcess process = CreateProcess();

        var result = process.Fail("   ");

        Assert.True(result.IsFailure);
        Assert.Equal("processing.error.required", result.Error.Code);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
    }

    [Fact]
    public void Fail_WhenAlreadyFailed_ReturnsValidationError()
    {
        VideoProcess process = CreateProcess();
        process.Fail("first failure");

        var result = process.Fail("second failure");

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    // ---------- Reset ----------

    [Fact]
    public void Reset_FromFailed_RestartsProcessAndClearsStepState()
    {
        VideoProcess process = CreateProcess();
        process.ProcessNextStep();
        process.CompleteCurrentStep();
        process.ProcessNextStep();
        process.CompleteCurrentStep("metadata"); // progress now 10
        process.Fail("transient failure");

        var result = process.Reset();

        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Null(process.CompletedAt);
        Assert.Null(process.ErrorMessage);
        Assert.False(process.IsCriticalError);
        Assert.All(process.Steps, s => Assert.Equal(StepStatus.PENDING, s.Status));
    }

    [Fact]
    public void Reset_WhenNotFailed_ReturnsValidationError()
    {
        VideoProcess process = CreateProcess();

        var result = process.Reset();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    // ---------- Retry ----------
    // NOTE: MaxRetries is never assigned (defaults to 0), so retries are
    // effectively disabled. These tests document the *current* behavior.

    [Fact]
    public void ScheduleRetry_WhenNotFailed_ReturnsValidationError()
    {
        VideoProcess process = CreateProcess();

        var result = process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    [Fact]
    public void ScheduleRetry_AfterCriticalFailure_ReturnsCriticalError()
    {
        VideoProcess process = CreateProcess();
        process.Fail("fatal", isCritical: true);

        var result = process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));

        Assert.True(result.IsFailure);
        Assert.Equal("processing.retry.critical", result.Error.Code);
    }

    [Fact]
    public void ScheduleRetry_AfterNonCriticalFailure_IsExhaustedBecauseMaxRetriesIsZero()
    {
        VideoProcess process = CreateProcess();
        process.Fail("transient", isCritical: false);

        var result = process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));

        Assert.True(result.IsFailure);
        Assert.Equal("processing.retry.exhausted", result.Error.Code);
    }

    [Fact]
    public void CanRetry_WithDefaultMaxRetries_ReturnsFalse()
    {
        VideoProcess process = CreateProcess();
        process.Fail("transient", isCritical: false);

        Assert.False(process.CanRetry());
    }

    // ---------- Helpers ----------

    // VideoProcess has no public factory (only private ctors for EF / internal use),
    // so tests build it through the private (Guid) constructor via reflection.
    private static VideoProcess CreateProcess(Guid? videoAssetId = null)
    {
        ConstructorInfo ctor = typeof(VideoProcess).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(Guid)],
            modifiers: null)!;

        return (VideoProcess)ctor.Invoke([videoAssetId ?? Guid.CreateVersion7()]);
    }
}
