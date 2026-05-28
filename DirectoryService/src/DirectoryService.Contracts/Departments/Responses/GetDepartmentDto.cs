namespace DirectoryService.Contracts.Departments.Responses;

public record GetDepartmentDto(
    Guid Id,
    string Name,
    Guid? ParentId,
    string Path,
    int Depth,
    int ChildrenCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);