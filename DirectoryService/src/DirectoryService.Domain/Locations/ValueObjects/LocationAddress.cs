using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

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
    
    public static Result<LocationAddress, Error> Create(
        string street, 
        string city, 
        string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsRequired("Street");
        
        if (string.IsNullOrWhiteSpace(city))
            return GeneralErrors.ValueIsRequired("City");
        
        if (string.IsNullOrWhiteSpace(country))
            return GeneralErrors.ValueIsRequired("Country");
        
        return new LocationAddress(street, city, country);
    }
    
    // Полный адрес
    public string FullAddress => $"{Street}, {City}, {Country}";
}