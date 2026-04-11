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

    public static Result<Position, Error> Create(string speciality, string direction, string? description, bool isActive)
    {
        var nameResult = PositionName.Create(speciality, direction);
        // if (nameResult.IsFailure)
        // {
        //     return Result.Failure<Position>(nameResult.Error);
        // }
        
        if (description is not null)
        {
            if (description.Length > MAX_LENGTH_1000)
            {
                return PositionErrors.ToLongDescription();
            }
        }
        
        var validName = nameResult.Value;
        
        return new Position(validName, description, isActive);
    }
}