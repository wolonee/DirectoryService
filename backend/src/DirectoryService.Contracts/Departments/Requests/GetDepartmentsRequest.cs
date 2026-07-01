using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Departments.Requests;

public record GetDepartmentsRequest(
    string? Search,
    Guid[]? LocationIds,
    bool? IsActive,
    string? SortBy,
    string? SortDir,
    PaginationRequest? Pagination);