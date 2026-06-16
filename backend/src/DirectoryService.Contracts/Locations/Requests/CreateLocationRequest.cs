namespace DirectoryService.Contracts.Locations.Requests;

public record CreateLocationRequest(
    CreateLocationAddressRequest Address,
    string Name,
    string Timezone);