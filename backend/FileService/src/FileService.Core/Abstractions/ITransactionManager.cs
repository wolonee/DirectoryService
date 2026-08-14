using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.Core.Abstractions;

public interface ITransactionManager
{
    public Task<Result<int, Error>> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    public Task<IDbTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}