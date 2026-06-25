using DirectoryService.Contracts.Positions.Dto;

namespace DirectoryService.Contracts.Positions.Responses;

public record GetPositionsResponse(List<GetPositionsDto> Positions, long TotalCount, int Page, int PageSize, int TotalPages);
