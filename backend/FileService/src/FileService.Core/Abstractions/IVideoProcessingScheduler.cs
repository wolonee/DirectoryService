using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.Assets;

namespace FileService.Core.Abstractions;

/// <summary>
/// Порт планирования фоновой обработки медиа-asset-а.
/// Core зависит только от этого интерфейса; конкретный планировщик (Quartz)
/// живёт в инфраструктуре (FileService.VideoProcessing).
/// </summary>
public interface IVideoProcessingScheduler
{
    Task<UnitResult<Error>> ScheduleAsync(MediaAsset mediaAsset, CancellationToken cancellationToken);
}
