using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    private PositionName(string speciality, string direction)
    {
        Speciality = speciality;
        Direction = direction;
    }
    
    public string Speciality { get; }
    public string Direction { get; }

    public static Result<PositionName> Create(string speciality, string direction)
    {
        const int MIN_LENGTH = 3;
        const int MAX_LENGTH = 120;
        
        int fullPositionLength = speciality.Length + direction.Length;

        if (string.IsNullOrWhiteSpace(speciality))
        {
            return Result.Failure<PositionName>("Speciality cannot be empty");
        }
        
        if (string.IsNullOrWhiteSpace(direction))
        {
            return Result.Failure<PositionName>("Direction cannot be empty");
        }

        if (fullPositionLength < MIN_LENGTH || fullPositionLength > MAX_LENGTH)
        {
            return Result.Failure<PositionName>("Speciality must be between 3 and 120 characters");
        }
        
        return new PositionName(speciality, direction);
    }
}