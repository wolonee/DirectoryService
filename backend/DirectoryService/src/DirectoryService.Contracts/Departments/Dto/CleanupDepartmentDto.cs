namespace DirectoryService.Contracts.Departments;

public record CleanupDepartmentDto
{
    public Guid Id { get; init; }

    public Guid? ParentId { get; init; }
    
    public string Identifier { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public int Depth { get; init; }
    
    public string ParentPath { get; init; } = string.Empty;
}