using FileService.Contracts.Features.Progress;
using FileService.Core.Features;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.MediaProcessing;
using Xunit;

namespace FileService.VideoProcessing.UnitTests;

public class ProgressEventMapperTests
{
    // Статус для клиента берётся от asset-а (MediaStatus), а не от VideoProcess.
    [Theory]
    [InlineData(MediaStatus.UPLOADING, ProcessStatus.QUEUED)]
    [InlineData(MediaStatus.UPLOADED, ProcessStatus.QUEUED)]
    [InlineData(MediaStatus.PROCESSING, ProcessStatus.PROCESSING)]
    [InlineData(MediaStatus.READY, ProcessStatus.READY)]
    [InlineData(MediaStatus.FAILED, ProcessStatus.FAILED)]
    [InlineData(MediaStatus.DELETED, ProcessStatus.FAILED)]
    public void ToDto_MapsMediaStatus_ToProcessStatus(MediaStatus mediaStatus, ProcessStatus expected)
    {
        VideoProcess process = CreateProcess();

        ProgressEventDto dto = ProgressEventMapper.ToDto(process, mediaStatus);

        Assert.Equal(expected, dto.ProcessStatus);
    }

    [Fact]
    public void ToDto_MapsIdentityFields()
    {
        Guid assetId = Guid.CreateVersion7();
        VideoProcess process = CreateProcess(assetId);

        ProgressEventDto dto = ProgressEventMapper.ToDto(process, MediaStatus.PROCESSING);

        Assert.Equal(assetId, dto.MediaAssetId);
        Assert.Equal(process.CountSteps(), dto.TotalSteps);    // 6 шагов
        Assert.Equal(process.ProgressPercentage, dto.Percent); // свежий процесс = 0
    }

    [Fact]
    public void ToDto_WithoutActiveStep_LeavesStepNull()
    {
        // Свежий процесс: ни один шаг ещё не IN_PROGRESS → CurrentStep == null.
        VideoProcess process = CreateProcess();

        ProgressEventDto dto = ProgressEventMapper.ToDto(process, MediaStatus.PROCESSING);

        Assert.Null(dto.StepOrder);
        Assert.Null(dto.StepName);
    }

    [Fact]
    public void ToDto_WithActiveStep_FillsStepOrderAndName()
    {
        VideoProcess process = CreateProcess();
        process.ProcessNextStep(); // стартует INITIALIZE (Order 1) → он становится активным

        ProgressEventDto dto = ProgressEventMapper.ToDto(process, MediaStatus.PROCESSING);

        Assert.Equal(1, dto.StepOrder);
        Assert.Equal("Initialize", dto.StepName);
    }

    [Fact]
    public void ToDto_AccumulatesPercent_ByCompletedStepWeights()
    {
        VideoProcess process = CreateProcess();

        // INITIALIZE (вес 0)
        process.ProcessNextStep();
        process.CompleteCurrentStep();
        // EXTRACT_METADATA (вес 10)
        process.ProcessNextStep();
        process.CompleteCurrentStep();

        ProgressEventDto dto = ProgressEventMapper.ToDto(process, MediaStatus.PROCESSING);

        Assert.Equal(10, dto.Percent);
    }

    [Fact]
    public void ToDto_ForFailedProcess_ExposesErrorMessage()
    {
        VideoProcess process = CreateProcess();
        process.Fail("ffmpeg crashed");

        ProgressEventDto dto = ProgressEventMapper.ToDto(process, MediaStatus.FAILED);

        Assert.Equal(ProcessStatus.FAILED, dto.ProcessStatus);
        Assert.Equal("ffmpeg crashed", dto.Error);
    }

    private static VideoProcess CreateProcess(Guid? assetId = null) =>
        new(assetId ?? Guid.CreateVersion7());
}
