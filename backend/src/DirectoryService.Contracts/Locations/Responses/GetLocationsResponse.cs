namespace DirectoryService.Contracts.Locations.Responses;

public record GetLocationsResponse(
    List<GetLocationDto> Locations,
    long TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
