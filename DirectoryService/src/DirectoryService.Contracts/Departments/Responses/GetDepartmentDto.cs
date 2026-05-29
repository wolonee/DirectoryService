namespace DirectoryService.Contracts.Departments.Responses;

public record GetTopDepartmentsDepartmentDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int Depth { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int CountPositions { get; init; }
    public List<GetTopDepartmentsDepartmentPositionDto> Positions { get; init; } = [];
}

public record GetTopDepartmentsDepartmentPositionDto
{
    public Guid Id { get; init; }
    public string Speciality { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}