namespace DirectoryService.Contracts.Locations;

public record PaginationRequest(int Page = 1, int PageSize = 2);