using System.Collections;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments;

public class Department
{
    // EF CORE
    private Department()
    {
    }
    
    private List<Department> _childrenDepartments = [];
    private List<DepartmentLocation> _departmentLocations = [];
    private List<DepartmentPosition> _departmentPositions = [];
    
    private Department(
        Guid? id,
        DepartmentName departmentName, 
        DepartmentIdentifier departmentIdentifier, 
        DepartmentPath departmentPath, 
        int depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = id ?? Guid.NewGuid();
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

    public static Result<Department, Error> CreateParent(
        Guid? id,
        DepartmentName name, 
        DepartmentIdentifier identifier, 
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        var departmentLocationsList = departmentLocations.ToList();
        
        var path = DepartmentPath.CreateParent(identifier);
        
        var createdDepartment = new Department(id, name, identifier, path, 0, departmentLocationsList);
        
        return createdDepartment;
    }
    
    public static Result<Department, Error> CreateChild(
        Guid? id,
        DepartmentName name, 
        DepartmentIdentifier identifier, 
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        var departmentLocationsList = departmentLocations.ToList();
        
        var path = parent.DepartmentPath.CreateChild(identifier);
        
        var createdDepartment = new Department(id, name, identifier, path, parent.Depth + 1, departmentLocationsList);
        
        parent._childrenDepartments.Add(createdDepartment);
        
        return createdDepartment;
    }
    
    public void AddDepartmentPosition(DepartmentPosition departmentPosition)
    {
        _departmentPositions.Add(departmentPosition);
        UpdatedAt = DateTime.UtcNow;
    }

    public UnitResult<Error> UpdateLocations(IReadOnlyList<DepartmentLocation> departmentLocations)
    {
        _departmentLocations = departmentLocations.ToList();
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}