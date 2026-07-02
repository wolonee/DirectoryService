using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Positions.Requests;

public record GetPositionsRequest(
    string? Search,
    string? SortBy,
    string? SortDir,
    PaginationRequest? Pagination);
