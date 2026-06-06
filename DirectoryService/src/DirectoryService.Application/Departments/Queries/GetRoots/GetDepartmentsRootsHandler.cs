using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Departments.Queries.GetRoots;

public class GetDepartmentsRootsHandler : IQueryHandler<GetDepartmentRootsResponse>
{
    public Task<Result<GetDepartmentRootsResponse, Errors>> Handle(CancellationToken cancellationToken = default)
    {
        
    }
}