using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions.ValueObjects;

public record PositionName
{
    private PositionName(string speciality, string direction)
    {
        Speciality = speciality;
        Direction = direction;
    }
    
    public string Speciality { get; }
    public string Direction { get; }

    public static Result<PositionName, Error> Create(string speciality, string direction)
    {
        const int MIN_LENGTH = 3;
        const int MAX_LENGTH = 100;
        
        int fullPositionLength = speciality.Length + direction.Length;

        if (string.IsNullOrWhiteSpace(speciality))
        {
            return GeneralErrors.ValueIsRequired("Speciality");
        }
        
        if (string.IsNullOrWhiteSpace(direction))
        {
            return GeneralErrors.ValueIsRequired("Direction");
        }

        if (fullPositionLength < MIN_LENGTH || fullPositionLength > MAX_LENGTH)
        {
            return GeneralErrors.ValueHasBoundedLength(MIN_LENGTH, MAX_LENGTH, "Speciality");
        }
        
        return new PositionName(speciality, direction);
    }
}