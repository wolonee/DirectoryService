using DirectoryService.Application.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Database;

public class DirectoryServiceDbContext : DbContext, IReadDbContext
{
    private readonly string _connectionString;
    
    public DirectoryServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public DbSet<DepartmentLocation> DepartmentLocations { get; set; }
    
    public DbSet<DepartmentPosition> DepartmentPositions { get; set; }
    
    public DbSet<Department> Departments { get; set; }
    
    public DbSet<Position> Positions { get; set; }
    
    public DbSet<Location> Locations { get; set; }
    
    public IQueryable<DepartmentLocation> DepartmentLocationsRead => DepartmentLocations.AsNoTracking().AsQueryable();
    
    public IQueryable<DepartmentPosition> DepartmentPositionsRead => DepartmentPositions.AsNoTracking().AsQueryable();
    
    public IQueryable<Department> DepartmentsRead => Departments.AsNoTracking().AsQueryable();
    
    public IQueryable<Position> PositionsRead => Positions.AsNoTracking().AsQueryable();
    
    public IQueryable<Location> LocationsRead => Locations.AsNoTracking().AsQueryable();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseNpgsql(_connectionString);

        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }
}