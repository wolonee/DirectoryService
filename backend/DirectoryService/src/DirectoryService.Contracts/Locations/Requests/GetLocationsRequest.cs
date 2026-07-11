using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Locations.Requests;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    int MinDepartmentCount,
    string? Search,
    string? Status,
    string? SortBy,
    string? SortDirection,
    PaginationRequest? Pagination = null);