using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Infrastructure;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    
    UnitResult<Error> Rollback();
}