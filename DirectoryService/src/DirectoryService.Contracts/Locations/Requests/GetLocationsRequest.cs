namespace DirectoryService.Contracts.Locations;

public record GetLocationsRequest(
    string? Search,
    string? Country,
    string? City,
    string? Street,
    bool? IsActive);