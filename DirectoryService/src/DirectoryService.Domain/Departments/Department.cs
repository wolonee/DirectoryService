using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class Department
{
    private List<Department> _children = [];
    private List<DepartmentLocation> _departmentLocations = [];
    
    private Department(
        Name name, 
        Identifier identifier, 
        Department? parent, 
        Path path, 
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
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
    
    public Guid Id { get; private set; }
    
    public Name Name { get; private set; }
    
    public Identifier Identifier { get; private set; }
    
    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations; // create location
    
    public Path Path { get; private set; }
    
    public int Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department> Create(
        string name, 
        string identifier, 
        Department? parent, 
        bool isActive)
    {
        var nameResult = Name.Create(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<Department>(nameResult.Error);
        }
        
        var identifierResult = Identifier.Create(identifier);
        if (identifierResult.IsFailure)
        {
            return Result.Failure<Department>(identifierResult.Error);
        }
        
        var validName = nameResult.Value;
        var validIdentifier = identifierResult.Value;
        
        string path;
        int depth;
        
        if (parent != null)
        {
            path = $"{parent.Path.Value}.{validIdentifier.Value}";
            depth = parent.Depth + 1;
        }
        else
        {
            path = validIdentifier.Value;
            depth = 1;
        }
        
        var pathResult = Path.Create(path);
        if (pathResult.IsFailure)
        {
            return Result.Failure<Department>(pathResult.Error);
        }
        
        var validPath = pathResult.Value;
        
        return new Department(validName, validIdentifier, parent, validPath, depth, isActive);
    }

    public Result Rename(string name, string identifier)
    {
        var nameResult = Name.Create(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }
            
        var identifierResult = Identifier.Create(identifier);
        if (identifierResult.IsFailure)
        {
            return Result.Failure(identifierResult.Error);
        }
        
        Name = nameResult.Value;
        Identifier = identifierResult.Value;
        
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

    // public DepartmentLocation AddLocation(Guid departmentId, Guid locationId)
    // {
    //     var departmentLocation = new DepartmentLocation(departmentId, locationId);
    //     _departmentLocations.Add(departmentLocation);
    //
    //     return departmentLocation;
    // }
}

public record Path
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    public Path(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<Path> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Path>("Path can't be empty");
        }

        if (value.StartsWith('.') || value.EndsWith('.') || value.Contains(".."))
        {
            return Result.Failure<Path>("Path cannot start or end with a dot");
        }

        return new Path(value);
    }
}