using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class DepartmentPosition
{
    private DepartmentPosition(Guid departmentId, Guid positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; private set; }
    
    public Guid DepartmentId { get; private set; }
    
    public Guid PositionId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public static Result<DepartmentPosition> Create(Guid departmentId, Guid positionId)
    {
        if (departmentId == Guid.Empty)
        {
            return Result.Failure<DepartmentPosition>("DepartmentId cannot be empty");
        }

        if (positionId == Guid.Empty)
        {
            return Result.Failure<DepartmentPosition>("PositionId cannot be empty");
        }

        return new DepartmentPosition(departmentId, positionId);
    }
}