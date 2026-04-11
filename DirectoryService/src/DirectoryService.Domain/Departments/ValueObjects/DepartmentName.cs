using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentName
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    
    private DepartmentName(string value) {
        Value = value;
    } 
    
    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Name");
        }
        
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueHasBoundedLength(MIN_LENGTH, MAX_LENGTH, "Name");
        }

        return new DepartmentName(value);
    }
}