using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateParent;

public class UpdateParentHandler : ICommandHandler<Guid, UpdateParentCommand>
{
    private readonly IValidator<UpdateParentCommand> _validator;
    private readonly ILogger<UpdateParentHandler> _logger;

    public UpdateParentHandler(IValidator<UpdateParentCommand> validator, ILogger<UpdateParentHandler> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateParentCommand command, CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();
        
        // Business validation
        
        // Update parent
        
        // Save in database

        // Logging about success result
        _logger.LogInformation("Department was successfully updated.");
    }
}