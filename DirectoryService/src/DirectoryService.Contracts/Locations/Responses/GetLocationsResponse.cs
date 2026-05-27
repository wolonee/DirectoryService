namespace DirectoryService.Contracts.Locations;

public record GetLocationsResponse(List<GetLocationDto> Locations, long TotalCount);