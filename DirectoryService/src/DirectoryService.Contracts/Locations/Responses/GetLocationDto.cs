namespace DirectoryService.Contracts.Locations.Responses;

public record GetLocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string Timezone { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int CountDepartments { get; init; }
}