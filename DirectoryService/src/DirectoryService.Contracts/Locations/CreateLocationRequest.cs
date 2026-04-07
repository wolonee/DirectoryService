namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(
    string Street,
    string City,
    string Country,
    string Name,
    string Timezone);