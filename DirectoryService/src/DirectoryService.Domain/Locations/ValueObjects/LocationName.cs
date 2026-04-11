using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record LocationName
{
    public const int MAX_LENGTH = 120;
    public const int MIN_LENGTH = 3;
    
    private LocationName(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired();
        }

        string normalized = Regex.Replace(value.Trim(), @"\s+", " ");

        if (normalized.Length < MIN_LENGTH || normalized.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueHasBoundedLength(MIN_LENGTH, MAX_LENGTH);
        }

        return new LocationName(normalized);
    }
}