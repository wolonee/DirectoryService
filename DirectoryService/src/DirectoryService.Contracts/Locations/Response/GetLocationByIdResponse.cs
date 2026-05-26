namespace DirectoryService.Contracts.Locations;

public record GetLocationByIdResponse
{
    public Guid Id { get; init; }
    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Timezone { get; init; } = string.Empty;
}