using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Infrastructure;

public interface ITransactionManager
{
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
    
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);
}