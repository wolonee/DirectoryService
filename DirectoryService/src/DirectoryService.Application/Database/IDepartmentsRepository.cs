using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Application.Database;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken = default);
    
    Task<Result<IReadOnlyList<Department>, Error>> GetActiveDepartmentsAsync(Guid[] departmentId, CancellationToken cancellationToken = default);
    
    Task<UnitResult<Error>> SaveAsync(CancellationToken cancellationToken = default);
}