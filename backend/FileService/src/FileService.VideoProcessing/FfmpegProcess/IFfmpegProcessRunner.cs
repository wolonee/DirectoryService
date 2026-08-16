using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities;

namespace FileService.VideoProcessing.FfmpegProcess;

public interface IFfmpegProcessRunner
{
    Task<Result<VideoMetadata, Error>> ExtractMetadataAsync(
        string inputFileUrl,
        CancellationToken ct = default);

    Task<UnitResult<Error>> GenerateHlsAsync(
        string inputFileUrl,
        string outputDirectory,
        CancellationToken ct = default);
}