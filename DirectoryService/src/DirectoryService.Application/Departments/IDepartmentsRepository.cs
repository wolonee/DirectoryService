using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken = default);
    
    Task<Result<List<Department>, Error>> GetActiveDepartmentsAsync(Guid[] departmentId, CancellationToken cancellationToken = default);

}