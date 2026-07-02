using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Common;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentPositions;

public record GetDepartmentPositionsQuery(
    Guid DepartmentId,
    PaginationRequest? Pagination = null) : IQuery;
