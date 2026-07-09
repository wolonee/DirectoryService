namespace DirectoryService.Contracts.Departments;

public class DepartmenDto
{
    public Guid Id { get; set; }

    public Guid? Parent { get; set; }

    public string Path { get; set; } = null!;

    public int Depth { get; set; }

    public bool IsActive { get; set; }

    public List<DepartmenDto> Children { get; set; } = [];
}