using DirectoryService.Contracts.Locations.Common;

namespace DirectoryService.Contracts.Departments.Requests;

public record GetDepartmentsRequest( 
    string? Search,
    string? SortBy,
    string? SortDir,
    PaginationRequest? Pagination);