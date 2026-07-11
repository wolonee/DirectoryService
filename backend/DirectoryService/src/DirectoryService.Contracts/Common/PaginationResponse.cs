namespace DirectoryService.Contracts.Common;

public record PaginationResponse<T>(List<T> Items, long TotalCount, int Page, int PageSize, int TotalPages);