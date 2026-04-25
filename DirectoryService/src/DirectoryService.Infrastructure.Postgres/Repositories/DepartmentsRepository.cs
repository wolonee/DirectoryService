using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _dbContext.Departments.AddAsync(department, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return department.Id;
    }

    public async Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);
        
        if (department is null)
            return GeneralErrors.NotFound(departmentId, "department");

        return department;
    }

    public async Task<Result<IReadOnlyList<Department>, Error>> GetActiveDepartmentsAsync(
        Guid[] departmentIds, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .Where(d => departmentIds.Contains(d.Id) && d.IsActive)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<UnitResult<Error>> DeleteLocationsByDepartmentId(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Locations
            .Where(x => x.Id == departmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
    
}