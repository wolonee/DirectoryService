using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.Get;

public class GetDepartmentsHandler : IQueryHandler<GetDepartmentsResponse, GetDepartmentsQuery>
{
    private readonly IValidator<GetDepartmentsQuery> _validator;
    private readonly IReadDbContext _context;
    private readonly ILogger<GetDepartmentsHandler> _logger;

    public GetDepartmentsHandler(
        IValidator<GetDepartmentsQuery> validator,
        IReadDbContext context,
        ILogger<GetDepartmentsHandler> logger)
    {
        _validator = validator;
        _context = context;
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

        var request = query.Request;
        var departmentsQuery = _context.DepartmentsRead;

        if (!string.IsNullOrWhiteSpace(request.Search))
            departmentsQuery = departmentsQuery.Where(d => d.DepartmentName.Value.Contains(request.Search, StringComparison.CurrentCultureIgnoreCase));
        
        

        var result = await departmentsQuery
            .Select(x => new GetDepartmentsDto
            {
                Id = x.Id,
                Name = x.DepartmentName.Value,
                Path = x.DepartmentPath.Value,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync(cancellationToken: cancellationToken);
        
        return new GetDepartmentsResponse(result);

    }
}