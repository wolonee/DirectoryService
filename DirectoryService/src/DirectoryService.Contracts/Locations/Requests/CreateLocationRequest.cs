namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(
    CreateLocationAddressRequest Address,
    string Name,
    string Timezone);