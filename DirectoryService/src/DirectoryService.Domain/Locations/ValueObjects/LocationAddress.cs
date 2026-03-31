using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public record LocationAddress
{
    private LocationAddress(
        string street,
        string city,
        string country)
    {
        Street = street;
        City = city;
        Country = country;
    }
    
    public string Street { get; } // Улица + дом
    public string City { get; }
    public string Country { get; }
    
    public static Result<LocationAddress> Create(
        string street, 
        string city, 
        string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<LocationAddress>("Street cannot be empty");
        
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<LocationAddress>("City cannot be empty");
        
        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<LocationAddress>("Country cannot be empty");
        
        return new LocationAddress(street, city, country);
    }
    
    // Полный адрес
    public string FullAddress => $"{Street}, {City}, {Country}";
}