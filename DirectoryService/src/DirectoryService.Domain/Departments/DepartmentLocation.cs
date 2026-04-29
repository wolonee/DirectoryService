using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments;

public class DepartmentLocation
{
    private DepartmentLocation()
    {
    }
    
    private DepartmentLocation(Department department, Guid locationId)
    {
        Department = department;
        DepartmentId = department.Id;
        LocationId = locationId;
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
    
    public static Result<DepartmentLocation, Error> Create(Department department, Guid locationId)
    {
        if (department.Id == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("DepartmentId");
        }

        if (locationId == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("LocationId");
        }

        return new DepartmentLocation(department, locationId);
    }
    
    public static Result<DepartmentLocation, Error> Create(Guid departmentId, Guid locationId)
    {
        if (departmentId == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("DepartmentId");
        }

        if (locationId == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("LocationId");
        }

        return new DepartmentLocation(departmentId, locationId);
    }
}