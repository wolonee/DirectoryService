namespace DirectoryService.Contracts.Departments.Responses;

public record GetDepartmentsResponse(List<GetDepartmentsDto> Departments, long TotalCount);