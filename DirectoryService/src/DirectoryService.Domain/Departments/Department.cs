using System.Collections;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

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
        Guid? parentId,
        DepartmentName departmentName, 
        DepartmentIdentifier departmentIdentifier, 
        DepartmentPath departmentPath, 
        int depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = id ?? Guid.NewGuid();
        DepartmentName = departmentName;
        DepartmentIdentifier = departmentIdentifier;
        ParentId = parentId ?? null;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        DepartmentPath = departmentPath;
        Depth = depth;
        ChildrenCount = ChildrenDepartments.Count;
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
        DepartmentName name, 
        DepartmentIdentifier identifier, 
        IEnumerable<DepartmentLocation> departmentLocations, 
        Guid? id)
    {
        var departmentLocationsList = departmentLocations.ToList();
        
        var path = DepartmentPath.CreateParent(identifier);
        
        var createdDepartment = new Department(id, null, name, identifier, path, 0, departmentLocationsList);
        
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
        
        var createdDepartment = new Department(id, parent.Id, name, identifier, path, parent.Depth + 1, departmentLocationsList);
        
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

    public void Activate(bool boolean)
    {
        IsActive = boolean;
        UpdatedAt = DateTime.UtcNow;
    }

    public UnitResult<Error> Deactivate()
    {
        if (!IsActive)
            return DepartmentErrors.IsAlreadyInactive();

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    // public UnitResult<Error> UpdateParentForChildren(Department parent)
    // {
    //     Depth = parent.Depth + 1;
    //     DepartmentPath = DepartmentPath.CreateChild(parent.DepartmentIdentifier);
    //     UpdatedAt = DateTime.UtcNow;
    // }
}