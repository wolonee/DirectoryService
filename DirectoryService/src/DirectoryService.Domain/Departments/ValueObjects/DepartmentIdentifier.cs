using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentIdentifier
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    public DepartmentIdentifier(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<DepartmentIdentifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DepartmentIdentifier>("Identifier can't be empty");
        }
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<DepartmentIdentifier>("Identifier must be between 3 and 150 characters");
        }

        return new DepartmentIdentifier(value);
    }
}