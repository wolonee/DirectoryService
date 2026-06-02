namespace DirectoryService.Contracts.Departments;

public class GetDepartmentsDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Path { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}