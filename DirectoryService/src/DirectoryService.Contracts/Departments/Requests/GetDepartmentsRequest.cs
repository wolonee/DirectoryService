namespace DirectoryService.Contracts.Departments.Requests;

public record GetDepartmentsRequest( 
    string? Search,
    string? SortBy,
    string? SortDir,
    int? Page,
    int? PageSize);