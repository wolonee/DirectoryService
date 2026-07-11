namespace DirectoryService.Contracts.Departments.Responses;

public record GetDepartmentPositionsDto
{
    public Guid Id { get; init; }
    public string Speciality { get; init; } = null!;
    public string Direction { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
