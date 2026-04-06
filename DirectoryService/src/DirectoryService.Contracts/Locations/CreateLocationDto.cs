namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(
    string Street,
    string City,
    string Country,
    string Name,
    string Timezone);