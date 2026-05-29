using DirectoryService.Contracts.Locations.Common;

namespace DirectoryService.Contracts.Locations.Requests;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection,
    PaginationRequest? Pagination = null);