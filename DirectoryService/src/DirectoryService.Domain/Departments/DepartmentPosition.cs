using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments;

public class DepartmentPosition
{
    private DepartmentPosition()
    {
    }
    
    private DepartmentPosition(Guid departmentId, Guid positionId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        PositionId = positionId;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; private set; }
    
    public Guid DepartmentId { get; private set; }
    
    public Department Department { get; private set; }
    
    public Guid PositionId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public static Result<DepartmentPosition, Error> Create(Guid departmentId, Guid positionId)
    {
        if (departmentId == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("DepartmentId");
        }

        if (positionId == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("PositionId");
        }

        return new DepartmentPosition(departmentId, positionId);
    }
}