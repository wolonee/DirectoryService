using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.MediaProcessing;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Handlers;

public sealed class InitializeStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.INITIALIZE;
    
    private readonly ILogger<InitializeStepHandler> _logger;
    
    public InitializeStepHandler(ILogger<InitializeStepHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Initializing video processing for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        UnitResult<Error> createResult = context.CreateWorkingDirectory();
        if (createResult.IsFailure)
            return Task.FromResult(Result.Failure<ProcessingContext, Error>(createResult.Error));

        _logger.LogDebug(
            "Working directory created: {WorkingDirectory}",
            context.WorkingDirectory);

        return Task.FromResult(
            Result.Success<ProcessingContext, Error>(context));
    }
}
