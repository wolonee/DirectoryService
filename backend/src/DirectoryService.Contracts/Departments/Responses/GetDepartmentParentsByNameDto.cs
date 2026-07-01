namespace DirectoryService.Contracts.Departments.Responses;

public record GetDepartmentParentsByNameWithParentsDto
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string Name { get; init; } = null!;
    public string Identifier { get; init; } = null!;
    public string Path { get; init; } = null!;
    public int Depth { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    public IEnumerable<GetDepartmentParentsByNameDto> Parents { get; init; } = null!;
}

public record GetDepartmentParentsByNameDto
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string Name { get; init; } = null!;
    public string Identifier { get; init; } = null!;
    public string Path { get; init; } = null!;
    public int Depth { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record GetDepartmentParentsByNameForSqlDto
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string Name { get; init; } = null!;
    public string Identifier { get; init; } = null!;
    public string Path { get; init; } = null!;
    public int Depth { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    public Guid AncestorId { get; init; }
    public Guid? AncestorParentId { get; init; }
    public string AncestorName { get; init; } = null!;
    public string AncestorIdentifier { get; init; } = null!;
    public string AncestorPath { get; init; } = null!;
    public int AncestorDepth { get; init; }
    public bool AncestorIsActive { get; init; }
    public DateTime AncestorCreatedAt { get; init; }
    public DateTime AncestorUpdatedAt { get; init; }
}