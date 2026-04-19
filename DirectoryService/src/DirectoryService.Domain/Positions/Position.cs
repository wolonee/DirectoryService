using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
    // EF CORE
    public const int MAX_LENGTH_1000 = 1000;
    
    private Position()
    {
    }
    
    private Position(PositionName name, PositionDescription? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
    
    public Guid Id { get; private set; }
    
    public PositionName Name { get; private set; }
    
    public PositionDescription? Description { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }

    public static Result<Position, Error> Create(PositionName name, PositionDescription? description = null)
    {
        return new Position(name, description);
    }
}