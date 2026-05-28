namespace DirectoryService.Contracts.Departments.Responses;

public record GetTopDepartmentsByPositionsResponse(
    List<GetDepartmentDto> Departments
    // list Positions
    // int CountPositions
);


// public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
