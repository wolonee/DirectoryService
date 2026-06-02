using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.Get;

public class GetDepartmentsHandler : IQueryHandler<GetDepartmentsResponse, GetDepartmentsQuery>
{
    private readonly ILogger<GetDepartmentsHandler> _logger;

    public GetDepartmentsHandler(
        ILogger<GetDepartmentsHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result<GetDepartmentsResponse, Errors>> Handle(
        GetDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        
    }
}