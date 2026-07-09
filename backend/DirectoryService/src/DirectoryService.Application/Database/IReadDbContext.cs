using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<DepartmentPosition> DepartmentPositionsRead { get; }
    
    IQueryable<Department> DepartmentsRead { get; }
    
    IQueryable<Position> PositionsRead { get; }
    
    IQueryable<Location> LocationsRead { get; }

}