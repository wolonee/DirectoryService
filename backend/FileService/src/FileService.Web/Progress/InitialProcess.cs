using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.Progress;
using FileService.Core.Abstractions;
using FileService.Core.Features;

namespace FileService.Web.Progress;

public class InitialProcess 
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IVideoProcessingRepository _videoProcessingRepository;

    public InitialProcess(
        IMediaAssetRepository mediaAssetRepository,
        IVideoProcessingRepository videoProcessingRepository)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _videoProcessingRepository = videoProcessingRepository;
    }

    public async Task<Result<ProgressEventDto, Error>> InitialProcessEvent(Guid mediaAssetId)
    {
        var videoProcessResult = await _videoProcessingRepository.GetBy(p => p.VideoAssetId == mediaAssetId);
        if (videoProcessResult.IsFailure)
            return videoProcessResult.Error;
        
        var videoProcess = videoProcessResult.Value;

        var mediaAssetResult = await _mediaAssetRepository.GetByIdAsync(videoProcess.VideoAssetId, CancellationToken.None);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;
        
        ProgressEventDto response = ProgressEventMapper.ToDto(videoProcess, mediaAssetResult.Value.Status);
        
        return response;
    }
}