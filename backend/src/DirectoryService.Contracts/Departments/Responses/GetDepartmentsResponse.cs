namespace DirectoryService.Contracts.Departments.Responses;

public record GetDepartmentsResponse(List<GetDepartmentsDto> Departments, long TotalCount, int Page, int PageSize, int TotalPages);