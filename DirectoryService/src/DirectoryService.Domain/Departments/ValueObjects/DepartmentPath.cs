using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentPath
{
    private const char SEPARATOR = '/';
    
    private DepartmentPath(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static DepartmentPath CreateParent(DepartmentIdentifier identifier)
    {
        return new DepartmentPath(identifier.Value);
    }
    
    public DepartmentPath CreateChild(DepartmentIdentifier identifier)
    {
        return new DepartmentPath(Value + SEPARATOR + identifier.Value);
    }
}