using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentParentsByName;

public class GetDepartmentParentsByNameHandler : IQueryHandler<GetDepartmentParentsByNameResponse, GetDepartmentParentsByNameQuery>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<GetDepartmentParentsByNameHandler> _logger;

    public GetDepartmentParentsByNameHandler(
        IDepartmentsRepository departmentsRepository,
        ILogger<GetDepartmentParentsByNameHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public Task<Result<GetDepartmentParentsByNameResponse, Errors>> Handle(
        GetDepartmentParentsByNameQuery query,
        CancellationToken cancellationToken = default)
    {
        
    }
}