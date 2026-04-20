using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions.ValueObjects;

public record PositionDescription
{
    private PositionDescription(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<PositionDescription, Error> Create(string? value)
    {
        const int MAX_LENGTH = 1000;

        if (string.IsNullOrWhiteSpace(value))
        {
            return new PositionDescription(string.Empty);
        }
        
        if (value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueHasBoundedLength(0, MAX_LENGTH, "Description");
        }
        
        return new PositionDescription(value);
    }
}