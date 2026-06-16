using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Seeder;

/// <summary>
/// Seeds a large, reproducible directory tree for development and load testing.
/// </summary>
public class DepartmentTreeSeeder
{
    private const int DepartmentCount = 10_000;
    private const int RootDepartmentCount = 10;
    private const int ChildrenPerDepartment = 5;
    private const int LocationCount = 300;
    private const int PositionCount = 500;
    private const int MinLocationsPerDepartment = 1;
    private const int LocationVariants = 3;
    private const int MinPositionsPerDepartment = 2;
    private const int PositionVariants = 3;

    private static readonly string[] Countries =
    [
        "Russia",
        "USA",
        "Germany",
        "United Kingdom",
        "Japan",
        "UAE",
        "India",
        "Singapore",
        "Brazil",
        "Canada"
    ];

    private static readonly string[] TimeZones =
    [
        "Europe/Moscow",
        "America/New_York",
        "Europe/Berlin",
        "Europe/London",
        "Asia/Tokyo",
        "Asia/Dubai",
        "Asia/Kolkata",
        "Asia/Singapore",
        "America/Sao_Paulo",
        "America/Toronto"
    ];

    private static readonly string[] PositionSpecialities =
    [
        "Software Engineer",
        "QA Engineer",
        "Product Manager",
        "Project Manager",
        "Business Analyst",
        "Data Engineer",
        "Data Scientist",
        "DevOps Engineer",
        "Security Engineer",
        "System Administrator",
        "UX Designer",
        "Technical Writer",
        "Recruiter",
        "HR Specialist",
        "Sales Manager",
        "Account Manager",
        "Marketing Specialist",
        "Financial Analyst",
        "Legal Counsel",
        "Support Engineer"
    ];

    private static readonly string[] PositionDirections =
    [
        "Backend",
        "Frontend",
        "Platform",
        "Infrastructure",
        "Analytics",
        "Operations",
        "Enterprise",
        "Regional",
        "Research",
        "Delivery"
    ];

    private readonly DirectoryServiceDbContext _context;

    public DepartmentTreeSeeder(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds locations, departments, positions, and their links when corresponding tables are empty.
    /// </summary>
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var locations = await GetOrCreateLocationsAsync(cancellationToken);

        List<Department> departments;
        if (!await _context.Departments.AnyAsync(cancellationToken))
        {
            departments = await CreateDepartmentsAsync(locations, cancellationToken);
        }
        else
        {
            departments = await _context.Departments.ToListAsync(cancellationToken);
        }

        if (!await _context.Positions.AnyAsync(cancellationToken))
        {
            await CreatePositionsWithDepartmentsAsync(departments, cancellationToken);
        }

        return departments.Count;
    }

    /// <summary>
    /// Removes all directory data in dependency order.
    /// </summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _context.DepartmentPositions.RemoveRange(_context.DepartmentPositions);
        _context.DepartmentLocations.RemoveRange(_context.DepartmentLocations);
        _context.Positions.RemoveRange(_context.Positions);
        _context.Departments.RemoveRange(_context.Departments);
        _context.Locations.RemoveRange(_context.Locations);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<Location>> GetOrCreateLocationsAsync(CancellationToken cancellationToken)
    {
        var existingLocations = await _context.Locations
            .Where(location => location.Name.Value.StartsWith("Seed Location"))
            .ToDictionaryAsync(location => location.Name.Value, cancellationToken);

        var locations = new List<Location>(LocationCount);
        var newLocations = new List<Location>(Math.Max(0, LocationCount - existingLocations.Count));

        for (var index = 0; index < LocationCount; index++)
        {
            var name = $"Seed Location {index + 1:D3}";
            if (existingLocations.TryGetValue(name, out var existingLocation))
            {
                locations.Add(existingLocation);
                continue;
            }

            var countryIndex = index % Countries.Length;
            var address = LocationAddress.Create(
                $"Business Street {index + 1}",
                $"City {countryIndex + 1:D2}-{(index / Countries.Length) + 1:D2}",
                Countries[countryIndex]).Value;
            var locationName = LocationName.Create(name).Value;
            var timeZone = LocationTimeZone.Create(TimeZones[countryIndex]).Value;
            var location = Location.Create(address, locationName, timeZone).Value;

            locations.Add(location);
            newLocations.Add(location);
        }

        if (newLocations.Count > 0)
        {
            await _context.Locations.AddRangeAsync(newLocations, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return locations;
    }

    private async Task<List<Department>> CreateDepartmentsAsync(
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken)
    {
        var departments = new List<Department>(DepartmentCount);

        for (var index = 0; index < DepartmentCount; index++)
        {
            var id = Guid.NewGuid();
            var locationIds = GetDepartmentLocationIds(index, locations);
            Result<Department, Error> result;

            if (index < RootDepartmentCount)
            {
                result = CreateParentDepartment(id, index, locationIds);
            }
            else
            {
                var parentIndex = (index - RootDepartmentCount) / ChildrenPerDepartment;
                result = CreateChildDepartment(id, index, departments[parentIndex], locationIds);
            }

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Department creation failed: {result.Error}");
            }

            departments.Add(result.Value);
        }

        await _context.Departments.AddRangeAsync(departments, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return departments;
    }

    private IReadOnlyList<Guid> GetDepartmentLocationIds(
        int departmentIndex,
        IReadOnlyList<Location> locations)
    {
        var linkCount = MinLocationsPerDepartment + (departmentIndex % LocationVariants);
        var locationIds = new List<Guid>(linkCount);

        for (var linkIndex = 0; linkIndex < linkCount; linkIndex++)
        {
            var locationIndex = ((departmentIndex * 17) + (linkIndex * 71)) % locations.Count;
            locationIds.Add(locations[locationIndex].Id);
        }

        return locationIds;
    }

    private Result<Department, Error> CreateParentDepartment(
        Guid id,
        int index,
        IEnumerable<Guid> locationIds)
    {
        var nameResult = DepartmentName.Create($"Root Department {index + 1:D2}");
        if (nameResult.IsFailure)
        {
            return nameResult.Error;
        }

        var identifierResult = DepartmentIdentifier.Create($"department_{index + 1:D5}");
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error;
        }

        var departmentLocations = locationIds
            .Select(locationId => DepartmentLocation.Create(id, locationId).Value)
            .ToList();

        return Department.CreateParent(nameResult.Value, identifierResult.Value, departmentLocations, id);
    }

    private Result<Department, Error> CreateChildDepartment(
        Guid id,
        int index,
        Department parent,
        IEnumerable<Guid> locationIds)
    {
        var nameResult = DepartmentName.Create($"Department {index + 1:D5}");
        if (nameResult.IsFailure)
        {
            return nameResult.Error;
        }

        var identifierResult = DepartmentIdentifier.Create($"department_{index + 1:D5}");
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error;
        }

        var departmentLocations = locationIds
            .Select(locationId => DepartmentLocation.Create(id, locationId).Value)
            .ToList();

        return Department.CreateChild(
            id,
            nameResult.Value,
            identifierResult.Value,
            parent,
            departmentLocations);
    }

    private async Task CreatePositionsWithDepartmentsAsync(
        IReadOnlyList<Department> departments,
        CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var positions = CreatePositions();
        var departmentPositions = new List<DepartmentPosition>(departments.Count * 3);

        for (var departmentIndex = 0; departmentIndex < departments.Count; departmentIndex++)
        {
            var department = departments[departmentIndex];
            var linkCount = MinPositionsPerDepartment + (departmentIndex % PositionVariants);

            for (var linkIndex = 0; linkIndex < linkCount; linkIndex++)
            {
                var positionIndex = ((departmentIndex * 13) + (linkIndex * 137)) % positions.Count;
                var position = positions[positionIndex];
                var linkResult = DepartmentPosition.Create(department.Id, position.Id);

                if (linkResult.IsFailure)
                {
                    throw new InvalidOperationException($"Department position creation failed: {linkResult.Error}");
                }

                var link = linkResult.Value;
                departmentPositions.Add(link);
            }
        }

        await _context.Positions.AddRangeAsync(positions, cancellationToken);
        await _context.DepartmentPositions.AddRangeAsync(departmentPositions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private List<Position> CreatePositions()
    {
        var positions = new List<Position>(PositionCount);

        for (var index = 0; index < PositionCount; index++)
        {
            var speciality = $"{PositionSpecialities[index % PositionSpecialities.Length]} {index + 1:D3}";
            var direction = PositionDirections[(index / PositionSpecialities.Length) % PositionDirections.Length];
            var nameResult = PositionName.Create(speciality, direction);
            var descriptionResult = PositionDescription.Create(
                $"Seeded position {index + 1:D3} for development and load testing.");

            if (nameResult.IsFailure)
            {
                throw new InvalidOperationException($"Position name creation failed: {nameResult.Error}");
            }

            if (descriptionResult.IsFailure)
            {
                throw new InvalidOperationException($"Position description creation failed: {descriptionResult.Error}");
            }

            var positionResult = Position.Create(Guid.NewGuid(), nameResult.Value, descriptionResult.Value);
            if (positionResult.IsFailure)
            {
                throw new InvalidOperationException($"Position creation failed: {positionResult.Error}");
            }

            positions.Add(positionResult.Value);
        }

        return positions;
    }
}
