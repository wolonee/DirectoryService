using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions.ValueObjects;

namespace DirectoryService.Domain.Positions;

public class Position
{
    // EF CORE
    private Position()
    {
    }
    
    private Position(PositionName name, string? description, bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
    
    public Guid Id { get; private set; }
    
    public PositionName Name { get; private set; }
    
    public string? Description { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }

    public static Result<Position> Create(string speciality, string direction, string? description, bool isActive)
    {
        var nameResult = PositionName.Create(speciality, direction);
        if (nameResult.IsFailure)
        {
            return Result.Failure<Position>(nameResult.Error);
        }
        
        if (description is not null)
        {
            if (description.Length > 1000)
            {
                return Result.Failure<Position>("Description is too long");
            }
        }
        
        var validName = nameResult.Value;
        
        return new Position(validName, description, isActive);
    }
}