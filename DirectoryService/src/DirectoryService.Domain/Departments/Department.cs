using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class Department
{
    private readonly List<Department> _children = [];
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];
    
    private Department(
        LocationName locationName, 
        DepartmentIdentifier departmentIdentifier, 
        Department? parent, 
        DepartmentPath departmentPath, 
        int depth, 
        bool isActive)
    {
        Id = Guid.NewGuid();
        LocationName = locationName;
        DepartmentIdentifier = departmentIdentifier;
        Parent = parent;
        DepartmentPath = departmentPath;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
    
    public Guid Id { get; private set; }
    
    public LocationName LocationName { get; private set; }
    
    public DepartmentIdentifier DepartmentIdentifier { get; private set; }
    
    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;
    
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
    
    public DepartmentPath DepartmentPath { get; private set; }
    
    public int Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department> Create(
        LocationName name, 
        DepartmentIdentifier identifier, 
        Department? parent, 
        bool isActive,
        IEnumerable<Guid> locationIds,
        IEnumerable<Guid> positionsIds)
    {
        string path;
        int depth;
        
        if (parent != null)
        {
            path = $"{parent.DepartmentPath.Value}.{identifier.Value}";
            depth = parent.Depth + 1;
        }
        else
        {
            path = identifier.Value;
            depth = 1;
        }
        
        var pathResult = DepartmentPath.Create(path);
        if (pathResult.IsFailure)
        {
            return Result.Failure<Department>(pathResult.Error);
        }
        
        var validPath = pathResult.Value;
        
        var createdDepartment = new Department(name, identifier, parent, validPath, depth, isActive);
        
        return createdDepartment;
    }

    // public Result UpdateLocations(IEnumerable<Guid> locationIds)
    // {
    //     foreach (var locationId in locationIds)
    //     {
    //         var addResult = AddDepartmentLocation(locationId);
    //         if (addResult.IsFailure)
    //         {
    //             return Result.Failure<Department>(addResult.Error);
    //         }
    //     }
    // }
    //
    // public Result AddDepartmentLocation(Guid locationId)
    // {
    //     var departmentLocationResult = DepartmentLocation.Create(Id, locationId);
    //     if (departmentLocationResult.IsFailure)
    //     {
    //         return Result.Failure(departmentLocationResult.Error);
    //     }
    //     
    //     _departmentLocations.Add(departmentLocationResult.Value);
    //     UpdatedAt = DateTime.UtcNow;
    //     
    //     return Result.Success();
    // }
    
    // public Result UpdatePositions(IEnumerable<Guid> positionsIds)
    // {
    //     foreach (var positionId in positionsIds)
    //     {
    //         var addResult = AddDepartmentPosition(positionId);
    //         if (addResult.IsFailure)
    //         {
    //             return Result.Failure<Department>(addResult.Error);
    //         }
    //     }
    // }
    //
    // public Result AddDepartmentPosition(Guid positionId)
    // {
    //     var departmentPositionResult = DepartmentPosition.Create(Id, positionId);
    //     if (departmentPositionResult.IsFailure)
    //     {
    //         return Result.Failure(departmentPositionResult.Error);
    //     }
    //     
    //     _departmentPositions.Add(departmentPositionResult.Value);
    //     UpdatedAt = DateTime.UtcNow;
    //     
    //     return Result.Success();
    // }

    public Result Rename(string name, string identifier)
    {
        var nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }
            
        var identifierResult = DepartmentIdentifier.Create(identifier);
        if (identifierResult.IsFailure)
        {
            return Result.Failure(identifierResult.Error);
        }
        
        LocationName = nameResult.Value;
        DepartmentIdentifier = identifierResult.Value;
        
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure("Department is already deactivated");
        }
        
        return Result.Success();
    }
    
    public Result Activate()
    {
        if (IsActive)
        {
            return Result.Failure("Department is already activated");
        }
        
        return Result.Success();
    }

    // public Result<DepartmentLocation> AddDepartmentLocation(Guid locationId)
    // {
    //     var departmentLocationResult = DepartmentLocation.Create(Id, locationId);
    //     if (departmentLocationResult.IsFailure)
    //     {
    //         return Result.Failure<DepartmentLocation>(departmentLocationResult.Error);
    //     }
    //     
    //     _departmentLocations.Add(departmentLocationResult.Value);
    //
    //     return departmentLocationResult.Value;
    // }
}