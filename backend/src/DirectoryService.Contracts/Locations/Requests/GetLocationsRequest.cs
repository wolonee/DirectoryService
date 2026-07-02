using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Locations.Requests;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    int MinDepartmentCount,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection,
    PaginationRequest? Pagination = null);