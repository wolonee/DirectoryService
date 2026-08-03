namespace DirectoryService.Contracts.Departments.Dto;

public class GetDepartmentsDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Path { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
