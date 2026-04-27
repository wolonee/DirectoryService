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
    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext, ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        _dbContext.Departments.Add(department);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return department.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return DepartmentErrors.NameConflict(department.DepartmentName.Value);
            }

            _logger.LogError(ex, "Database update error while creating department with name {Name}", department.DepartmentName.Value);
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating department with name {Name}", department.DepartmentName.Value);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department with name {Name}", department.DepartmentName.Value);
            return GeneralErrors.DatabaseError();
        }
    }
    
    public async Task<Result<List<Department>, Error>> GetAsync(
        Expression<Func<Department, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Departments.AsQueryable();
        
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }
        
            var departments = await query.ToListAsync(cancellationToken);
            return departments;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while getting departments");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting departments");
            return GeneralErrors.DatabaseError();
        }
    }
    
    public async Task<Result<Department, Error>> GetFirstAsync(
        Expression<Func<Department, bool>>? predicate = null,
        Func<IQueryable<Department>, IQueryable<Department>>? include = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Departments.AsQueryable();
        
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }
            
            if (include is not null)
            {
                query = include(query);
            }
        
            var department = await query.FirstOrDefaultAsync(cancellationToken);
        
            if (department is null)
            {
                return GeneralErrors.NotFound();
            }
        
            return department;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while getting department");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting department");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<Department, Error>> GetByIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var departmentResult = await GetFirstAsync(x => x.Id == departmentId, cancellationToken: cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;
        
        var entry = _dbContext.Entry(departmentResult.Value);
        var isTracked = entry.State != EntityState.Detached;    
        _logger.LogInformation("Department tracked: {Tracked}, State: {State}", isTracked, entry.State);
        
        return departmentResult.Value;
    }    

    public async Task<Result<IReadOnlyList<Department>, Error>> GetActiveDepartmentsAsync(
        Guid[] departmentIds,
        CancellationToken cancellationToken = default)
    {
        var departmentsResult = await GetAsync(
            dep => departmentIds.Contains(dep.Id) && dep.IsActive,
            cancellationToken: cancellationToken);
        
        if (departmentsResult.IsFailure)
            return departmentsResult.Error;

        return departmentsResult.Value;
    }
    
    public async Task<Result<Department, Error>> GetActiveDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var departmentsResult = await GetFirstAsync(
            dep => dep.Id == departmentId && dep.IsActive,
            query => query.Include(x => x.ChildrenDepartments),
            cancellationToken: cancellationToken);
        
        if (departmentsResult.IsFailure)
            return departmentsResult.Error;

        return departmentsResult.Value;
    }
    
    public async Task<Result<Department, Error>> GetActiveParentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var departmentsResult = await GetFirstAsync(
            dep => dep.Id == departmentId && dep.IsActive,
            cancellationToken: cancellationToken);
        
        if (departmentsResult.IsFailure)
            return departmentsResult.Error;

        return departmentsResult.Value;
    }

    public async Task<UnitResult<Error>> DeleteLocationsByDepartmentId(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.DepartmentLocations
            .Where(x => x.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);
        
        return UnitResult.Success<Error>();
    }
}