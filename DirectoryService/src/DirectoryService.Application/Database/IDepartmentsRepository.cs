using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Database;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken = default);
    
    Task<Result<IReadOnlyList<Department>, Error>> GetActiveDepartmentsAsync(Guid[] departmentId, CancellationToken cancellationToken = default);
    
    Task<UnitResult<Error>> DeleteLocationsByDepartmentId(Guid departmentId, CancellationToken cancellationToken = default);
    
    Task<Result<Department, Error>> GetActiveDepartmentWithLock(Guid departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> LockDescendants(string rootPath, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateParent(
        string rootPath,
        string newParentPath,
        Guid oldParentId,
        Guid? newParentId,
        CancellationToken cancellationToken = default);

    Task<Result<bool, Error>> HasActiveChildDepartmentsAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<Result<bool, Error>> DepartmentPositionExistsAsync(
        Guid departmentId,
        Guid positionId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> AddDepartmentPositionAsync(
        DepartmentPosition departmentPosition,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteDepartmentPositionAsync(
        Guid departmentId,
        Guid positionId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteDepartmentPositionsByDepartmentIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteDepartmentPositionsByPositionIdAsync(
        Guid positionId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateParentInCleanupDelete(
        string rootPath,
        string newParentPath,
        Guid departmentId,
        Guid? newParentId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteDepartmentInCleanupDelete(
        Guid departmentId,
        CancellationToken cancellationToken = default);
}