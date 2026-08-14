using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities.Assets;
using FileService.Domain.S3Entities.MediaProcessing;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing;

public class ProcessingPipeline : IProcessingPipeline
{
    private readonly IEnumerable<IProcessingStepHandler> _stepHandlers;
    private readonly ILogger<ProcessingPipeline> _logger;
    private readonly IVideoProcessingRepository _videoProcessingRepository;
    private readonly IMediaAssetRepository _mediaAssetsRepository;
    private readonly IVideoAssetRepository _videoAssetRepository;
    private readonly ITransactionManager _transactionManager;


    public ProcessingPipeline(
        ILogger<ProcessingPipeline> logger,
        IVideoProcessingRepository videoProcessingRepository,
        IMediaAssetRepository mediaAssetsRepository,
        IVideoAssetRepository videoAssetRepository,
        ITransactionManager transactionManager,
        IEnumerable<IProcessingStepHandler> stepHandlers)
    {
        _logger = logger;
        _videoProcessingRepository = videoProcessingRepository;
        _mediaAssetsRepository = mediaAssetsRepository;
        _videoAssetRepository = videoAssetRepository;
        _transactionManager = transactionManager;
        _stepHandlers = stepHandlers;
    }
    
    public async Task<UnitResult<Error>> ProcessAllStepsAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        Result<ProcessingContext, Error> contextResult = await LoadContextAsync(videoAssetId, cancellationToken);
        if (contextResult.IsFailure)
            return contextResult.Error;

        ProcessingContext context = contextResult.Value;
        var videoAsset = context.VideoAsset;

        while (true)
        {
            Result<ProcessingStep?, Error> stepResult = context.VideoProcess.ProcessNextStep();
            if (stepResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to process next step for VideoAssetId: {VideoAssetId}. Status: {Status}",
                    videoAssetId,
                    context.VideoProcess.Status);

                return stepResult.Error;
            }
            
            if (stepResult.Value is null)
            {
                _logger.LogInformation(
                    "All processing steps completed for VideoAssetId: {VideoAssetId}",
                    videoAssetId);
                
                var completeResult = videoAsset.CompleteProcessing(context.StorageReference, DateTime.UtcNow);
                if (completeResult.IsFailure)
                    return completeResult.Error;

                return UnitResult.Success<Error>();
            }

            ProcessingStep currentStep = stepResult.Value;

            _logger.LogInformation(
                "Processing step {StepType} (Order: {Order}) for VideoAssetId: {VideoAssetId}",
                currentStep.StepType,
                currentStep.Order,
                videoAssetId);
            
            IProcessingStepHandler? stepHandler = _stepHandlers.FirstOrDefault(h => h.StepType == currentStep.StepType);
            
            if (stepHandler is null)
            {
                string error = $"No handler found for step type {currentStep.StepType}";
                _logger.LogError("No handler found for step type {StepType}", currentStep.StepType);

                context.VideoProcess.FailCurrentStep(error);
                context.VideoProcess.Fail(error, isCritical: true);
                context.VideoAsset.MarkFailed(DateTime.UtcNow);

                UnitResult<Error> saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to save context after missing handler for step {StepType} for VideoAssetId: {VideoAssetId}",
                        currentStep.StepType,
                        videoAssetId);
                }

                return Error.Failure("pipeline.handler.not.found", error);
            }
            
            Result<ProcessingContext, Error> executionResult = await ExecuteStepSafelyAsync(
                stepHandler,
                context,
                cancellationToken);

            if (executionResult.IsFailure)
            {
                _logger.LogError(
                    "Step {StepType} failed for VideoAssetId: {VideoAssetId}. Error: {Error}",
                    currentStep.StepType,
                    videoAssetId,
                    executionResult.Error);

                context.VideoProcess.FailCurrentStep(executionResult.Error.Message);
                context.VideoProcess.Fail(executionResult.Error.Message, isCritical: true);
                context.VideoAsset.MarkFailed(DateTime.UtcNow);

                UnitResult<Error> saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to save context after step failure {StepType} for VideoAssetId: {VideoAssetId}",
                        currentStep.StepType,
                        videoAssetId);
                }

                return executionResult.Error;
            }
            
            context = executionResult.Value;

            context.VideoProcess.CompleteCurrentStep();

            _logger.LogInformation(
                "Step {StepType} completed for VideoAssetId: {VideoAssetId}. Progress: {Progress}%",
                currentStep.StepType,
                videoAssetId,
                context.VideoProcess.ProgressPercentage);
    
            UnitResult<Error> completeSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (completeSaveResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to save progress after step {StepType} for VideoAssetId: {VideoAssetId}",
                    currentStep.StepType,
                    videoAssetId);

                return completeSaveResult.Error;
            }
        }
    }
    
    private async Task<Result<ProcessingContext, Error>> ExecuteStepSafelyAsync(
        IProcessingStepHandler handler,
        ProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return await handler.ExecuteAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception in step handler {StepType} for VideoAssetId: {VideoAssetId}",
                handler.StepType,
                context.VideoAsset.Id);

            return Error.Failure("pipeline.step.exception", $"Step execution failed: {ex.Message}");
        }
    }
    
    private async Task<Result<ProcessingContext, Error>> LoadContextAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        Result<VideoProcess, Error> processingResult = await _videoProcessingRepository
            .GetBy(vp => vp.VideoAssetId == videoAssetId, cancellationToken);

        VideoProcess videoProcess;

        if (processingResult.IsFailure)
        {
            var newProcess = new VideoProcess(videoAssetId);
            videoProcess = newProcess;

            _videoProcessingRepository.Add(videoProcess);

            _logger.LogInformation("Created new VideoProcessing for VideoAssetId: {VideoAssetId}", videoAssetId);
        }
        else
        {
            videoProcess = processingResult.Value;

            _logger.LogInformation(
                "Loaded existing VideoProcess for VideoAssetId: {VideoAssetId}",
                videoAssetId);
        }

        Result<VideoAsset, Error> assetResult = await _videoAssetRepository.GetByIdAsync(videoAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        UnitResult<Error> startResult = assetResult.Value.StartProcessing();
        if (startResult.IsFailure)
            return startResult.Error;
        
        Result<int, Error> saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        var processingContext = new ProcessingContext
        {
            VideoAsset = assetResult.Value,
            VideoProcess = videoProcess,
        };

        return processingContext;
        
    }
}