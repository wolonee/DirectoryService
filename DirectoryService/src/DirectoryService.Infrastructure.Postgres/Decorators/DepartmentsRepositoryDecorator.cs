using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Decorators;

public class DepartmentsRepositoryDecorator : IDepartmentsRepository
{
    private readonly DepartmentsRepository _innerRepo;
    private readonly ILogger<DepartmentsRepositoryDecorator> _logger;

    public DepartmentsRepositoryDecorator(DepartmentsRepository innerRepo, ILogger<DepartmentsRepositoryDecorator> logger)
    {
        _innerRepo = innerRepo;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentIdResult = await _innerRepo.AddAsync(department, cancellationToken);
            
            if (departmentIdResult.IsFailure)
                return departmentIdResult.Error;
            
            return departmentIdResult.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return DepartmentErrors.NameConflict(department.DepartmentName.Value);
            }

            _logger.LogError(ex, "Database error while creating department '{Name}'", department.DepartmentName.Value);
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation cancelled while creating department '{Name}'", department.DepartmentName.Value);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department '{Name}'", department.DepartmentName.Value);
            return GeneralErrors.DatabaseError();
        }
    }
    
    public async Task<Result<Department, Error>> GetByIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentResult = await _innerRepo.GetByIdAsync(departmentId, cancellationToken);
            if (departmentResult.IsFailure)
                return departmentResult.Error;
            
            return departmentResult.Value;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while GetByIdAsync department with id {Id}", departmentId);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while GetByIdAsync department with id {Id}", departmentId);
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<IReadOnlyList<Department>, Error>> GetActiveDepartmentsAsync(
        Guid[] departmentIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentsResult = await _innerRepo.GetActiveDepartmentsAsync(departmentIds, cancellationToken);
            if (departmentsResult.IsFailure)
                return departmentsResult.Error;

            return departmentsResult;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while GetActiveDepartmentsAsync");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while GetActiveDepartmentsAsync");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<UnitResult<Error>> DeleteLocationsByDepartmentId(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteLocationResult = await _innerRepo.DeleteLocationsByDepartmentId(departmentId, cancellationToken: cancellationToken);
            if (deleteLocationResult.IsFailure)
                return deleteLocationResult.Error;

            return deleteLocationResult;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            _logger.LogError(ex, "Database update error while DeleteLocationsByDepartmentId in department with id {Id}", departmentId);
            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while DeleteLocationsByDepartmentId in department with id {Id}", departmentId);
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while DeleteLocationsByDepartmentId in department with id {Id}", departmentId);
            return GeneralErrors.DatabaseError();
        }
    }
}