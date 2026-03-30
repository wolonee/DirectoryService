using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record DepartmentName
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    public DepartmentName(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DepartmentName>("Name can't be empty");
        }
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<DepartmentName>("Name must be between 3 and 150 characters");
        }

        return new DepartmentName(value);
    }
}