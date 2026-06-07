using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentParentsByName;

public class GetDepartmentParentsByNameHandler : IQueryHandler<GetDepartmentParentsByNameResponse, GetDepartmentParentsByNameQuery>
{
    private readonly IValidator<GetDepartmentParentsByNameQuery> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<GetDepartmentParentsByNameHandler> _logger;

    public GetDepartmentParentsByNameHandler(
        IValidator<GetDepartmentParentsByNameQuery> validator,
        IDepartmentsRepository departmentsRepository,
        ILogger<GetDepartmentParentsByNameHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<GetDepartmentParentsByNameResponse, Errors>> Handle(
        GetDepartmentParentsByNameQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
    }
}