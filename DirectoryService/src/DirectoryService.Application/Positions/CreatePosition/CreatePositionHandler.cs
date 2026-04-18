using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(IValidator<CreatePositionCommand> validator, ILogger<CreatePositionHandler> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Create Department Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }
        
        // business validation
        
        
        // create position
        
        // save position in db
        
        // logger about success save
    }
}