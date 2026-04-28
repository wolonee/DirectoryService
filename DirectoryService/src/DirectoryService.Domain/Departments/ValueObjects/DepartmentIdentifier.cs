using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentIdentifier
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 2;
    
    private static readonly Regex ValidLtreeRegex = new(@"^[A-Za-z0-9_]+$");
    
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
        value = Regex.Replace(value, @"\s+", "_");
        
        value = Regex.Replace(value, @"[^A-Za-z0-9_]", "");
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueHasBoundedLength(MIN_LENGTH, MAX_LENGTH, "Identifier");
        }
        
        if (!ValidLtreeRegex.IsMatch(value))
        {
            return GeneralErrors.ValueContainsInvalidCharacters("Only letters, numbers, and underscores are allowed for LTree path");
        }
        
        if (value.StartsWith('_') || value.EndsWith('_'))
        {
            return GeneralErrors.ValueContainsInvalidCharacters("Identifier cannot start or end with underscore");
        }
        
        return new DepartmentIdentifier(value);
    }
}