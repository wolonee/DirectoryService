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
/// Класс для сидирования тестовых данных дерева департаментов и позиций
/// </summary>
public class DepartmentTreeSeeder
{
    private readonly DirectoryServiceDbContext _context;

    public DepartmentTreeSeeder(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Сидирует дерево департаментов и позиции
    /// </summary>
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var totalAdded = 0;

        // 1. Локации
        var locationsMap = await GetOrCreateLocationsAsync(cancellationToken);

        // 2. Департаменты
        List<Department> departments;
        if (!await _context.Departments.AnyAsync(cancellationToken))
        {
            departments = await CreateDepartmentsAsync(locationsMap, cancellationToken);
            totalAdded += departments.Count;
        }
        else
        {
            departments = await _context.Departments
                .Include(d => d.DepartmentLocations)
                .ToListAsync(cancellationToken);
        }

        // 3. Позиции и связи с департаментами
        if (!await _context.Positions.AnyAsync(cancellationToken))
        {
            var positionsAdded = await CreatePositionsWithDepartmentsAsync(departments, cancellationToken);
            totalAdded += positionsAdded;
        }

        return totalAdded;
    }

    // ==================== ЛОКАЦИИ ====================
    private async Task<Dictionary<string, Guid>> GetOrCreateLocationsAsync(CancellationToken cancellationToken)
    {
        var map = new Dictionary<string, Guid>();

        // Moscow
        var moscow = await GetOrCreateLocation(
            "Tverskaya str. 1", "Moscow", "Russia", "Moscow HQ", "Europe/Moscow",
            cancellationToken);
        map["Moscow"] = moscow.Id;

        // Saint Petersburg
        var spb = await GetOrCreateLocation(
            "Nevsky prospect 10", "Saint Petersburg", "Russia", "Saint Petersburg Office", "Europe/Moscow",
            cancellationToken);
        map["SPb"] = spb.Id;

        // Kazan
        var kazan = await GetOrCreateLocation(
            "Baumana str. 5", "Kazan", "Russia", "Kazan Branch", "Europe/Moscow",
            cancellationToken);
        map["Kazan"] = kazan.Id;

        // HQ (Headquarters)
        var hq = await GetOrCreateLocation(
            "Main Square 1", "Moscow", "Russia", "Headquarters", "Europe/Moscow",
            cancellationToken);
        map["HQ"] = hq.Id;

        return map;
    }

    private async Task<Location> GetOrCreateLocation(
        string street, string city, string country, string name, string timeZone,
        CancellationToken cancellationToken)
    {
        var existing = await _context.Locations.FirstOrDefaultAsync(l => l.Name.Value == name, cancellationToken);
        if (existing != null) return existing;

        var address = LocationAddress.Create(street, city, country).Value;
        var locationName = LocationName.Create(name).Value;
        var tz = LocationTimeZone.Create(timeZone).Value;
        var location = Location.Create(address, locationName, tz).Value;

        await _context.Locations.AddAsync(location, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return location;
    }

    // ==================== ДЕПАРТАМЕНТЫ ====================
    private async Task<List<Department>> CreateDepartmentsAsync(Dictionary<string, Guid> locs, CancellationToken ct)
    {
        var all = new List<Department>();

        // ---- Корень ----
        var company = CreateParentDepartment(
            id: Guid.NewGuid(),
            name: "Company",
            identifier: "company",
            locationIds: [locs["HQ"]]
        ).Value;
        all.Add(company);

        // ---- Уровень 1 ----
        var engineering = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Engineering",
            identifier: "engineering",
            parent: company,
            locationIds: [locs["Moscow"], locs["SPb"]]
        ).Value;
        all.Add(engineering);

        var hr = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Human Resources",
            identifier: "hr",
            parent: company,
            locationIds: [locs["Moscow"]]
        ).Value;
        all.Add(hr);

        var sales = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Sales",
            identifier: "sales",
            parent: company,
            locationIds: [locs["SPb"], locs["Kazan"]]
        ).Value;
        all.Add(sales);

        var marketing = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Marketing",
            identifier: "marketing",
            parent: company,
            locationIds: [locs["Moscow"], locs["Kazan"]]
        ).Value;
        all.Add(marketing);

        var finance = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Finance",
            identifier: "finance",
            parent: company,
            locationIds: [locs["Moscow"]]
        ).Value;
        all.Add(finance);

        // ---- Уровень 2 (под Engineering) ----
        var backend = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Backend Development",
            identifier: "backend-dev",
            parent: engineering,
            locationIds: [locs["Moscow"]]
        ).Value;
        all.Add(backend);

        var frontend = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Frontend Development",
            identifier: "frontend-dev",
            parent: engineering,
            locationIds: [locs["SPb"]]
        ).Value;
        all.Add(frontend);

        var qa = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Quality Assurance",
            identifier: "qa",
            parent: engineering,
            locationIds: [locs["Moscow"], locs["Kazan"]]
        ).Value;
        all.Add(qa);

        var devops = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "DevOps",
            identifier: "devops",
            parent: engineering,
            locationIds: [locs["Moscow"], locs["SPb"]]
        ).Value;
        all.Add(devops);

        var dataScience = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Data Science",
            identifier: "data-science",
            parent: engineering,
            locationIds: [locs["Moscow"]]
        ).Value;
        all.Add(dataScience);

        // ---- Уровень 3 (под Backend) ----
        var apiTeam = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "API Team",
            identifier: "api-team",
            parent: backend,
            locationIds: [locs["Moscow"]]
        ).Value;
        all.Add(apiTeam);

        var databaseTeam = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Database Team",
            identifier: "database-team",
            parent: backend,
            locationIds: [locs["Moscow"]]
        ).Value;
        all.Add(databaseTeam);

        var integrationTeam = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Integration Team",
            identifier: "integration-team",
            parent: backend,
            locationIds: [locs["SPb"]]
        ).Value;
        all.Add(integrationTeam);

        // ---- Уровень 2 под Sales ----
        var ecommerceSales = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "E-commerce Sales",
            identifier: "ecommerce-sales",
            parent: sales,
            locationIds: [locs["Moscow"]]
        ).Value;
        all.Add(ecommerceSales);

        var retailSales = CreateChildDepartment(
            id: Guid.NewGuid(),
            name: "Retail Sales",
            identifier: "retail-sales",
            parent: sales,
            locationIds: [locs["SPb"], locs["Kazan"]]
        ).Value;
        all.Add(retailSales);

        await _context.Departments.AddRangeAsync(all, ct);
        await _context.SaveChangesAsync(ct);
        return all;
    }

    private Result<Department, Error> CreateParentDepartment(
        Guid id, string name, string identifier, IEnumerable<Guid> locationIds)
    {
        var nameRes = DepartmentName.Create(name);
        if (nameRes.IsFailure) return nameRes.Error;
        var idRes = DepartmentIdentifier.Create(identifier);
        if (idRes.IsFailure) return idRes.Error;

        var deptLocs = locationIds
            .Select(locId => DepartmentLocation.Create(id, locId).Value)
            .ToList();

        return Department.CreateParent(nameRes.Value, idRes.Value, deptLocs, id);
    }

    private Result<Department, Error> CreateChildDepartment(
        Guid id, string name, string identifier, Department parent, IEnumerable<Guid> locationIds)
    {
        var nameRes = DepartmentName.Create(name);
        if (nameRes.IsFailure) return nameRes.Error;
        var idRes = DepartmentIdentifier.Create(identifier);
        if (idRes.IsFailure) return idRes.Error;

        var deptLocs = locationIds
            .Select(locId => DepartmentLocation.Create(id, locId).Value)
            .ToList();

        return Department.CreateChild(id, nameRes.Value, idRes.Value, parent, deptLocs);
    }

    // ==================== ПОЗИЦИИ ====================
    private async Task<int> CreatePositionsWithDepartmentsAsync(List<Department> departments, CancellationToken ct)
    {
        var positions = new List<Position>();
        var departmentPositions = new List<DepartmentPosition>();

        var deptByIdentifier = departments.ToDictionary(d => d.DepartmentIdentifier.Value);

        var positionDefs = new[]
        {
            // ---- Engineering & Backend ----
            new {
                Speciality = "Software Engineer",
                Direction = "Backend",
                Description = "Разработка серверной логики, API и микросервисов",
                DepartmentIds = new[] { "backend-dev", "api-team", "database-team" }
            },
            new {
                Speciality = "Senior Software Engineer",
                Direction = "Backend",
                Description = "Архитектура и разработка сложных бэкенд-систем",
                DepartmentIds = new[] { "backend-dev", "api-team" }
            },
            new {
                Speciality = "Software Engineer",
                Direction = "Frontend",
                Description = "Разработка пользовательских интерфейсов на React/Vue",
                DepartmentIds = new[] { "frontend-dev" }
            },
            new {
                Speciality = "QA Engineer",
                Direction = "Automation",
                Description = "Написание автотестов, поддержка CI/CD для тестирования",
                DepartmentIds = new[] { "qa", "backend-dev", "frontend-dev" }
            },
            new {
                Speciality = "QA Manual",
                Direction = "Testing",
                Description = "Ручное тестирование, составление тест-кейсов",
                DepartmentIds = new[] { "qa", "frontend-dev" }
            },
            new {
                Speciality = "DevOps Engineer",
                Direction = "Infrastructure",
                Description = "CI/CD, Docker, Kubernetes, облачные провайдеры",
                DepartmentIds = new[] { "devops", "backend-dev" }
            },
            new {
                Speciality = "Data Scientist",
                Direction = "Analytics",
                Description = "Построение ML-моделей, очистка данных, эксперименты",
                DepartmentIds = new[] { "data-science" }
            },
            new {
                Speciality = "Data Engineer",
                Direction = "ETL",
                Description = "Построение пайплайнов данных, работа с Big Data",
                DepartmentIds = new[] { "data-science", "backend-dev" }
            },
            new {
                Speciality = "Security Engineer",
                Direction = "Cybersecurity",
                Description = "Аудит безопасности, пентесты, защита приложений",
                DepartmentIds = new[] { "devops", "backend-dev" }
            },
            new {
                Speciality = "System Administrator",
                Direction = "Infrastructure",
                Description = "Администрирование серверов, сетей, баз данных",
                DepartmentIds = new[] { "devops" }
            },

            // ---- Management & Leadership ----
            new {
                Speciality = "Project Manager",
                Direction = "Delivery",
                Description = "Управление проектами, коммуникация с заказчиком, планирование",
                DepartmentIds = new[] { "backend-dev", "frontend-dev", "qa", "data-science" }
            },
            new {
                Speciality = "Product Owner",
                Direction = "Product",
                Description = "Управление бэклогом, требования к продукту",
                DepartmentIds = new[] { "backend-dev", "frontend-dev" }
            },
            new {
                Speciality = "Tech Lead",
                Direction = "Backend",
                Description = "Техлид команды, код-ревью, архитектура",
                DepartmentIds = new[] { "backend-dev" }
            },
            new {
                Speciality = "Scrum Master",
                Direction = "Agile",
                Description = "Фасилитация, agile-коучинг, устранение блокеров",
                DepartmentIds = new[] { "backend-dev", "frontend-dev", "qa" }
            },

            // ---- HR ----
            new {
                Speciality = "HR Generalist",
                Direction = "HR",
                Description = "Кадровое делопроизводство, адаптация, оценка",
                DepartmentIds = new[] { "hr" }
            },
            new {
                Speciality = "Recruiter",
                Direction = "Talent Acquisition",
                Description = "Поиск и наём сотрудников, работа с каналами",
                DepartmentIds = new[] { "hr" }
            },
            new {
                Speciality = "Learning & Development Specialist",
                Direction = "Training",
                Description = "Организация обучения, развитие персонала",
                DepartmentIds = new[] { "hr" }
            },

            // ---- Sales & Marketing ----
            new {
                Speciality = "Sales Manager",
                Direction = "B2B",
                Description = "Продажи юридическим лицам, переговоры",
                DepartmentIds = new[] { "sales", "ecommerce-sales", "retail-sales" }
            },
            new {
                Speciality = "Account Manager",
                Direction = "Customer Success",
                Description = "Ведение клиентов, увеличение LTV",
                DepartmentIds = new[] { "sales", "ecommerce-sales" }
            },
            new {
                Speciality = "Sales Representative",
                Direction = "B2C",
                Description = "Активные продажи физическим лицам",
                DepartmentIds = new[] { "retail-sales" }
            },
            new {
                Speciality = "Marketing Specialist",
                Direction = "Digital",
                Description = "SEO, контекстная реклама, SMM",
                DepartmentIds = new[] { "marketing" }
            },
            new {
                Speciality = "Content Manager",
                Direction = "Content",
                Description = "Создание контента, копирайтинг, блоги",
                DepartmentIds = new[] { "marketing" }
            },
            new {
                Speciality = "PR Manager",
                Direction = "Communications",
                Description = "Связи с общественностью, пресс-релизы",
                DepartmentIds = new[] { "marketing" }
            },

            // ---- Finance ----
            new {
                Speciality = "Financial Analyst",
                Direction = "Budgeting",
                Description = "Анализ бюджета, план-факт, отчёты",
                DepartmentIds = new[] { "finance" }
            },
            new {
                Speciality = "Accountant",
                Direction = "Accounting",
                Description = "Бухгалтерский учёт, налоги, зарплата",
                DepartmentIds = new[] { "finance" }
            },
            new {
                Speciality = "Business Analyst",
                Direction = "Requirements",
                Description = "Сбор требований, анализ бизнес-процессов",
                DepartmentIds = new[] { "backend-dev", "frontend-dev", "qa" }
            }
        };

        foreach (var def in positionDefs)
        {
            var nameResult = PositionName.Create(def.Speciality, def.Direction);
            if (nameResult.IsFailure) throw new Exception($"PositionName error: {nameResult.Error}");

            var descResult = PositionDescription.Create(def.Description);
            if (descResult.IsFailure) throw new Exception($"PositionDescription error: {descResult.Error}");

            var positionId = Guid.NewGuid();
            var positionResult = Position.Create(positionId, nameResult.Value, descResult.Value);
            if (positionResult.IsFailure) throw new Exception($"Position creation error: {positionResult.Error}");

            var position = positionResult.Value;

            foreach (var deptId in def.DepartmentIds)
            {
                if (!deptByIdentifier.TryGetValue(deptId, out var department))
                    continue;

                var deptPosResult = DepartmentPosition.Create(department.Id, positionId);
                if (deptPosResult.IsFailure) throw new Exception($"DepartmentPosition creation error: {deptPosResult.Error}");

                var deptPos = deptPosResult.Value;

                position.AddDepartmentPosition(deptPos);
                department.AddDepartmentPosition(deptPos);
                departmentPositions.Add(deptPos);
            }

            positions.Add(position);
        }

        await _context.Positions.AddRangeAsync(positions, ct);
        await _context.SaveChangesAsync(ct);

        if (departmentPositions.Any())
        {
            await _context.DepartmentPositions.AddRangeAsync(departmentPositions, ct);
            await _context.SaveChangesAsync(ct);
        }

        return positions.Count;
    }

    /// <summary>
    /// Полная очистка сидированных данных
    /// </summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _context.DepartmentPositions.RemoveRange(_context.DepartmentPositions);
        _context.DepartmentLocations.RemoveRange(_context.DepartmentLocations);
        _context.Positions.RemoveRange(_context.Positions);
        _context.Departments.RemoveRange(_context.Departments);
        await _context.SaveChangesAsync(cancellationToken);
    }
}