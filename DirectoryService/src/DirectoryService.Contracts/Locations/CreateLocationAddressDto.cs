namespace DirectoryService.Contracts.Locations;

public record CreateLocationAddressDto(
    string Street,
    string City,
    string Country,
    string Name,
    string Timezone,
    bool IsActive);