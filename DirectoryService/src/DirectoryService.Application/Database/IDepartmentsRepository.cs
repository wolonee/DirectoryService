using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

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
        Guid newParentId,
        CancellationToken cancellationToken = default);
}