namespace DirectoryService.Contracts.Departments.Responses;

public record GetTopDepartmentsByPositionsResponse(
    List<GetTopDepartmentsDepartmentDto> Departments
);


// public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
