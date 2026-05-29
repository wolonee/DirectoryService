using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Database;

public interface ITransactionManager
{
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);

    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken, IsolationLevel? level = null);

}