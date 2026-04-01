using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentPath
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    public DepartmentPath(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<DepartmentPath> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DepartmentPath>("Path can't be empty");
        }

        if (value.StartsWith('.') || value.EndsWith('.') || value.Contains(".."))
        {
            return Result.Failure<DepartmentPath>("Path cannot start or end with a dot");
        }

        return new DepartmentPath(value);
    }
}