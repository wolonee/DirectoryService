using CSharpFunctionalExtensions;
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
                return LocationErrors.NameConflict(department.DepartmentName.Value);
            }

            _logger.LogError(ex, "Database update error while creating department with name {Name}", department.DepartmentName.Value);
            return LocationErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating department with name {Name}", department.DepartmentName.Value);
            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department with name {Name}", department.DepartmentName.Value);
            return LocationErrors.DatabaseError();
        }
    }
    
    public async Task<Result<Department, Error>> GetById(Guid departmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await _dbContext.Departments
                .FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);

            if (department is null)
            {
                return GeneralErrors.NotFound(departmentId);
            }
            
            return department;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating department with id {Id}", departmentId);
            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department with name {Id}", departmentId);
            return LocationErrors.DatabaseError();
        }
    }
}