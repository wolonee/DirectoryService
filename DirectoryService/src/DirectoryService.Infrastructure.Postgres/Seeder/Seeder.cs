namespace DirectoryService.Infrastructure.Seeder;

using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Класс для сидирования тестовых данных дерева департаментов
/// </summary>
public class DepartmentTreeSeeder
{
    private readonly DirectoryServiceDbContext _context;

    public DepartmentTreeSeeder(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Сидирует дерево департаментов с тестовыми данными
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Количество созданных департаментов</returns>
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Departments.AnyAsync(cancellationToken))
        {
            return 0;
        }

        var departmentsToSeed = new List<Department>();
        var locationsMap = await GetOrCreateLocationsAsync(cancellationToken);

        // Корневой департамент - Компания
        var companyResult = CreateParentDepartment(
            id: Guid.NewGuid(),
            name: "Company",
            identifier: "company",
            locationIds: [locationsMap["HQ"]]
        );

        if (companyResult.IsFailure)
        {
            throw new Exception($"Failed to create company department: {companyResult.Error}");
        }

        var company = companyResult.Value;
        departmentsToSeed.Add(company);

        // Департаменты первого уровня
        var engineeringResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Engineering",
            identifier: "engineering",
            parent: company,
            locationIds: [locationsMap["Moscow"], locationsMap["SPb"]]
        );

        if (engineeringResult.IsFailure)
        {
            throw new Exception($"Failed to create engineering department: {engineeringResult.Error}");
        }

        var engineering = engineeringResult.Value;
        departmentsToSeed.Add(engineering);

        var hrResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Human Resources",
            identifier: "hr",
            parent: company,
            locationIds: [locationsMap["Moscow"]]
        );

        if (hrResult.IsFailure)
        {
            throw new Exception($"Failed to create hr department: {hrResult.Error}");
        }

        var hr = hrResult.Value;
        departmentsToSeed.Add(hr);

        var salesResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Sales",
            identifier: "sales",
            parent: company,
            locationIds: [locationsMap["SPb"], locationsMap["Kazan"]]
        );

        if (salesResult.IsFailure)
        {
            throw new Exception($"Failed to create sales department: {salesResult.Error}");
        }

        var sales = salesResult.Value;
        departmentsToSeed.Add(sales);

        // Департаменты второго уровня (подразделения Engineering)
        var backendResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Backend Development",
            identifier: "backend-dev",
            parent: engineering,
            locationIds: [locationsMap["Moscow"]]
        );

        if (backendResult.IsFailure)
        {
            throw new Exception($"Failed to create backend department: {backendResult.Error}");
        }

        var backend = backendResult.Value;
        departmentsToSeed.Add(backend);

        var frontendResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Frontend Development",
            identifier: "frontend-dev",
            parent: engineering,
            locationIds: [locationsMap["SPb"]]
        );

        if (frontendResult.IsFailure)
        {
            throw new Exception($"Failed to create frontend department: {frontendResult.Error}");
        }

        var frontend = frontendResult.Value;
        departmentsToSeed.Add(frontend);

        var qaResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Quality Assurance",
            identifier: "qa",
            parent: engineering,
            locationIds: [locationsMap["Moscow"], locationsMap["Kazan"]]
        );

        if (qaResult.IsFailure)
        {
            throw new Exception($"Failed to create qa department: {qaResult.Error}");
        }

        var qa = qaResult.Value;
        departmentsToSeed.Add(qa);

        // Департаменты третьего уровня (команды Backend)
        var apiTeamResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "API Team",
            identifier: "api-team",
            parent: backend,
            locationIds: [locationsMap["Moscow"]]
        );

        if (apiTeamResult.IsFailure)
        {
            throw new Exception($"Failed to create api team department: {apiTeamResult.Error}");
        }

        var apiTeam = apiTeamResult.Value;
        departmentsToSeed.Add(apiTeam);

        var databaseTeamResult = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Database Team",
            identifier: "database-team",
            parent: backend,
            locationIds: [locationsMap["Moscow"]]
        );

        if (databaseTeamResult.IsFailure)
        {
            throw new Exception($"Failed to create database team department: {databaseTeamResult.Error}");
        }

        var databaseTeam = databaseTeamResult.Value;
        departmentsToSeed.Add(databaseTeam);

        // Сохраняем все департаменты
        await _context.Departments.AddRangeAsync(departmentsToSeed, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return departmentsToSeed.Count;
    }

    /// <summary>
    /// Создает или получает существующие локации для сидирования
    /// </summary>
    private async Task<Dictionary<string, Guid>> GetOrCreateLocationsAsync(CancellationToken cancellationToken)
    {
        var locationsMap = new Dictionary<string, Guid>();

        // Moscow
        var moscowAddress = LocationAddress.Create("Tverskaya str. 1", "Moscow", "Russia").Value;
        var moscowName = LocationName.Create("Moscow HQ").Value;
        var moscowTimezone = LocationTimeZone.Create("Europe/Moscow").Value;

        var existingMoscow = await _context.Locations.FirstOrDefaultAsync(
            l => l.Name.Value == "Moscow HQ",
            cancellationToken);

        if (existingMoscow == null)
        {
            var moscowLocation = Location.Create(moscowAddress, moscowName, moscowTimezone).Value;
            await _context.Locations.AddAsync(moscowLocation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            locationsMap["Moscow"] = moscowLocation.Id;
        }
        else
        {
            locationsMap["Moscow"] = existingMoscow.Id;
        }

        // SPb
        var spbAddress = LocationAddress.Create("Nevsky prospect 10", "Saint Petersburg", "Russia").Value;
        var spbName = LocationName.Create("Saint Petersburg Office").Value;
        var spbTimezone = LocationTimeZone.Create("Europe/Moscow").Value;

        var existingSpb = await _context.Locations.FirstOrDefaultAsync(
            l => l.Name.Value == "Saint Petersburg Office",
            cancellationToken);

        if (existingSpb == null)
        {
            var spbLocation = Location.Create(spbAddress, spbName, spbTimezone).Value;
            await _context.Locations.AddAsync(spbLocation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            locationsMap["SPb"] = spbLocation.Id;
        }
        else
        {
            locationsMap["SPb"] = existingSpb.Id;
        }

        // Kazan
        var kazanAddress = LocationAddress.Create("Baumana str. 5", "Kazan", "Russia").Value;
        var kazanName = LocationName.Create("Kazan Branch").Value;
        var kazanTimezone = LocationTimeZone.Create("Europe/Moscow").Value;

        var existingKazan = await _context.Locations.FirstOrDefaultAsync(
            l => l.Name.Value == "Kazan Branch",
            cancellationToken);

        if (existingKazan == null)
        {
            var kazanLocation = Location.Create(kazanAddress, kazanName, kazanTimezone).Value;
            await _context.Locations.AddAsync(kazanLocation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            locationsMap["Kazan"] = kazanLocation.Id;
        }
        else
        {
            locationsMap["Kazan"] = existingKazan.Id;
        }

        // HQ (Headquarters)
        var hqAddress = LocationAddress.Create("Main Square 1", "Moscow", "Russia").Value;
        var hqName = LocationName.Create("Headquarters").Value;
        var hqTimezone = LocationTimeZone.Create("Europe/Moscow").Value;

        var existingHq = await _context.Locations.FirstOrDefaultAsync(
            l => l.Name.Value == "Headquarters",
            cancellationToken);

        if (existingHq == null)
        {
            var hqLocation = Location.Create(hqAddress, hqName, hqTimezone).Value;
            await _context.Locations.AddAsync(hqLocation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            locationsMap["HQ"] = hqLocation.Id;
        }
        else
        {
            locationsMap["HQ"] = existingHq.Id;
        }

        return locationsMap;
    }

    /// <summary>
    /// Создает корневой департамент
    /// </summary>
    private Result<Department, Error> CreateParentDepartment(
        Guid id,
        string name,
        string identifier,
        IEnumerable<Guid> locationIds)
    {
        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
        {
            return nameResult.Error;
        }

        var identifierResult = DepartmentIdentifier.Create(identifier);
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error;
        }

        var departmentLocations = locationIds
            .Select(locId => DepartmentLocation.Create(id, locId).Value)
            .ToList();

        var parentResult = Department.CreateParent(
            id,
            nameResult.Value,
            identifierResult.Value,
            departmentLocations
        );

        return parentResult;
    }

    /// <summary>
    /// Создает дочерний департамент
    /// </summary>
    private Result<Department, Error> CreateChildDepartment(
        Guid id,
        string name,
        string identifier,
        Department parent,
        IEnumerable<Guid> locationIds)
    {
        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
        {
            return nameResult.Error;
        }

        var identifierResult = DepartmentIdentifier.Create(identifier);
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error;
        }

        var departmentLocations = locationIds
            .Select(locId => DepartmentLocation.Create(id, locId).Value)
            .ToList();

        var childResult = Department.CreateChild(
            id,
            nameResult.Value,
            identifierResult.Value,
            parent,
            departmentLocations
        );

        return childResult;
    }

    /// <summary>
    /// Очищает все сидированные данные
    /// </summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _context.DepartmentPositions.RemoveRange(_context.DepartmentPositions);
        _context.DepartmentLocations.RemoveRange(_context.DepartmentLocations);
        _context.Departments.RemoveRange(_context.Departments);
        await _context.SaveChangesAsync(cancellationToken);
    }
}