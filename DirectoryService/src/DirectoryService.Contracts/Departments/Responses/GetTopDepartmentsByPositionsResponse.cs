namespace DirectoryService.Contracts.Departments.Responses;

public record GetTopDepartmentsByPositionsResponse(
    Guid Id,
    string Name,
    Guid? ParentId,
    string Path,
    int Depth,
    int ChildrenCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
    // list Positions
    // int CountPositions
);



// public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
