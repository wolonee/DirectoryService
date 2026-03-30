using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class Department
{
    private List<Department> _children = [];
    private List<DepartmentLocation> _departmentLocations = [];
    
    private Department(
        string name, 
        string identifier, 
        Department? parent, 
        string path, 
        int depth, 
        bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = depth;
        IsActive = isActive;
    }
    
    public Guid Id { get; private set; }
    
    public string Name { get; private set; }
    
    public string Identifier { get; private set; }
    
    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations; // create location
    
    public string Path { get; private set; }
    
    public int Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = CreatedAt.Date

    public static Result<Department> Create(
        string name, 
        string identifier, 
        Department? parent, 
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
        {
            return Result.Failure<Department>("Name is too long or empty");
        }

        if (string.IsNullOrWhiteSpace(identifier) || identifier.Length > 150)
        {
            return Result.Failure<Department>("Identifier is too long or empty");
        }

        string path;
        int depth;
        
        if (parent != null)
        {
            path = $"{parent.Path}.{identifier}";
            depth = parent.Depth + 1;
        }
        else
        {
            path = identifier;
            depth = 1;
        }
        
        return new Department(name, identifier, parent, path, depth, isActive);
    }
}



public class Location
{
    public Guid Id { get; private set; }
    
    public string Name { get; private set; } // Типо: офисное здание PLAZA
    
    public string Contry { get; private set; }
    
    public string City { get; private set; }
    
    public string Address { get; private set; }
}



public class DepartmentLocation
{
    public Guid Id { get; private set; }
    
    public Guid DepartmentId { get; private set; }
    
    public Guid LocationId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
}
