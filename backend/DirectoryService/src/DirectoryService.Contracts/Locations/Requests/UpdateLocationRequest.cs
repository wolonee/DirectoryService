namespace DirectoryService.Contracts.Locations.Requests;

public record UpdateLocationRequest(
    CreateLocationAddressRequest Address,
    string Name,
    string Timezone);
