using System.Text.RegularExpressions;
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
        value = Regex.Replace(value, @"\s+", "-");
        
        if (value.Any(c => !char.IsAsciiLetter(c) && c != '-'))
        {
            return GeneralErrors.ValueContainsInvalidCharacters("Name");
        }
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueHasBoundedLength(MIN_LENGTH, MAX_LENGTH, "Identifier");
        }
        
        if (value.StartsWith('-') || value.EndsWith('-') || value.Contains("--"))
        {
            return GeneralErrors.ValueContainsInvalidCharacters("Name");
        }
        
        return new DepartmentIdentifier(value);
    }
}