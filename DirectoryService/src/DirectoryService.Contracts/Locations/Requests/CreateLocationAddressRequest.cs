namespace DirectoryService.Contracts.Locations;

public record class CreateLocationAddressRequest(string Country, string City, string Street);