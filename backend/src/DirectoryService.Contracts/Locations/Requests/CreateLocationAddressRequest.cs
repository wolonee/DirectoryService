namespace DirectoryService.Contracts.Locations.Requests;

public record class CreateLocationAddressRequest(string Country, string City, string Street);