using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class DepartmentLocation
{
    private DepartmentLocation(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; private set; }
    
    public Guid DepartmentId { get; private set; }
    
    public Guid LocationId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }


}