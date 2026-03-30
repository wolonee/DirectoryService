using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class Location
{
    private List<DepartmentLocation> _departmentLocations = [];

    private Location(
        LocationAddress locationAddress,
        string name,
        string timezone,
        bool isActive)
    {
        Id = Guid.NewGuid();
        LocationAddress = locationAddress;
        Name = name;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public LocationAddress LocationAddress { get; private set; }

    public string Name { get; private set; }

    public string Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocation => _departmentLocations;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(string address, string name, string timezone, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return
        }
    }
}