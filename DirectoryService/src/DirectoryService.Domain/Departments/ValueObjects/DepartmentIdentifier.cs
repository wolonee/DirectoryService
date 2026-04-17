using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentIdentifier
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    private DepartmentIdentifier(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<DepartmentIdentifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Identifier");
        }
        
        value = value.Trim();
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueHasBoundedLength(MIN_LENGTH, MAX_LENGTH, "Identifier");
        }

        if (value.Any(c => !char.IsAsciiLetter(c)))
        {
            return GeneralErrors.ValueContainsInvalidCharacters("Identifier");
        }
        
        return new DepartmentIdentifier(value);
    }
}