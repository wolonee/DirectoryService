using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record Identifier
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    public Identifier(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<Identifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Identifier>("Identifier can't be empty");
        }
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<Identifier>("Identifier must be between 3 and 150 characters");
        }

        return new Identifier(value);
    }
}