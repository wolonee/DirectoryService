namespace DirectoryService.Presentation.Controllers;

public record CreateLocationAddressDto(
    string Street,
    string City,
    string Country,
    string Timezone,
    bool IsActive);