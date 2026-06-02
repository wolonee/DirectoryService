using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.Get;

public class GetDepartmentsHandler : IQueryHandler<GetDepartmentsResponse, GetDepartmentsQuery>
{
    private readonly IValidator<GetDepartmentsQuery> _validator;
    private readonly ILogger<GetDepartmentsHandler> _logger;

    public GetDepartmentsHandler(
        IValidator<GetDepartmentsQuery> validator,
        ILogger<GetDepartmentsHandler> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetDepartmentsResponse, Errors>> Handle(
        GetDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Get Locations Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }
        
    }
}