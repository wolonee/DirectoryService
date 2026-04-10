using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public class DepartmentLocation
{
    private DepartmentLocation()
    {
    }
    
    private DepartmentLocation(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
    
    public Guid Id { get; private set; }
    
    public Guid DepartmentId { get; private set; }
    
    public Department Department { get; private set; }
    
    public Guid LocationId { get; private set; }
    
    public static Result<DepartmentLocation> Create(Guid departmentId, Guid locationId)
    {
        if (departmentId == Guid.Empty)
        {
            return Result.Failure<DepartmentLocation>("DepartmentId cannot be empty");
        }

        if (locationId == Guid.Empty)
        {
            return Result.Failure<DepartmentLocation>("LocationId cannot be empty");
        }

        return new DepartmentLocation(departmentId, locationId);
    }
}