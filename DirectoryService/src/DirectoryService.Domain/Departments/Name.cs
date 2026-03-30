using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record Name
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    public Name(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Name>("Name can't be empty");
        }
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<Name>("Name must be between 3 and 150 characters");
        }

        return new Name(value);
    }
}