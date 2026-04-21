using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
{
    private readonly IValidator<UpdateLocationsCommand> _validator;
    private readonly ILogger<UpdateLocationsHandler> _logger;

    public UpdateLocationsHandler(IValidator<UpdateLocationsCommand> validator, ILogger<UpdateLocationsHandler> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken = default)
    {
        // FluentValidation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToValidationErrors();

        // Business validation

        // Update Locations

        // Save in database

        // Logging about success result
    }
}