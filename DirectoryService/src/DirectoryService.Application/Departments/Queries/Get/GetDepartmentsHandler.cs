using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Departments.Queries.Get;

public class GetDepartmentsHandler : IQueryHandler<GetDepartmentsResponse, GetDepartmentsQuery>
{
    public Task<Result<GetDepartmentsResponse, Errors>> Handle(
        GetDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        
    }
}