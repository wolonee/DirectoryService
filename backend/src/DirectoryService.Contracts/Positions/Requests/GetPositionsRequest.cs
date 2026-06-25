using DirectoryService.Contracts.Locations.Common;

namespace DirectoryService.Contracts.Positions.Requests;

public record GetPositionsRequest(
    string? Search,
    string? SortBy,
    string? SortDir,
    PaginationRequest? Pagination);
