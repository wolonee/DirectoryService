using System.Collections;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class Department
{
    // EF CORE
    private Department()
    {
    }
    
    private readonly List<Department> _childrenDepartments = [];
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];
    
    private Department(
        DepartmentName departmentName, 
        DepartmentIdentifier departmentIdentifier, 
        DepartmentPath departmentPath, 
        int depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = Guid.NewGuid();
        DepartmentName = departmentName;
        DepartmentIdentifier = departmentIdentifier;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        DepartmentPath = departmentPath;
        Depth = depth;
        _departmentLocations = departmentLocations.ToList();
    }
    
    public Guid Id { get; private set; }
    
    public DepartmentName DepartmentName { get; private set; }
    
    public DepartmentIdentifier DepartmentIdentifier { get; private set; }
    
    public Guid? ParentId { get; private set; }
    
    public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;
    
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
    
    public DepartmentPath DepartmentPath { get; private set; }
    
    public int Depth { get; private set; }
    
    public int ChildrenCount { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department> CreateParent(
        DepartmentName name, 
        DepartmentIdentifier identifier, 
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Result.Failure<Department>("Locations can't be empty");
        }
        
        var path = DepartmentPath.CreateParent(identifier);
        
        var createdDepartment = new Department(name, identifier, path, 0, departmentLocationsList);
        
        return createdDepartment;
    }
    
    public static Result<Department> CreateChild(
        DepartmentName name, 
        DepartmentIdentifier identifier, 
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Result.Failure<Department>("Locations can't be empty");
        }
        
        var path = parent.DepartmentPath.CreateChild(identifier);
        
        var createdDepartment = new Department(name, identifier, path, parent.Depth + 1, departmentLocationsList);
        
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
        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }
            
        var identifierResult = DepartmentIdentifier.Create(identifier);
        if (identifierResult.IsFailure)
        {
            return Result.Failure(identifierResult.Error);
        }
        
        DepartmentName = nameResult.Value;
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