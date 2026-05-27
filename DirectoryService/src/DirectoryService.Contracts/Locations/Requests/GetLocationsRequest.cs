namespace DirectoryService.Contracts.Locations;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    int? Page,
    int? PageSize);