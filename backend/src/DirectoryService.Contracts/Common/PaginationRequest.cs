namespace DirectoryService.Contracts.Locations.Common;

public record PaginationRequest(int Page = 1, int PageSize = 20);