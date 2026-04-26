using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Infrastructure;

public class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    
    public TransactionScope(IDbTransaction transaction)
    {
        _transaction = transaction;
    }
    
    public UnitResult<Error> Commit()
    {
        try
        {
            _transaction.Commit();
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            return Error.Failure("transaction.commit.failed", "Failed to commit transaction");
        }
    }
    
    public UnitResult<Error> Rollback()
    {
        try
        {
            _transaction.Rollback();
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            return Error.Failure("transaction.rollback.failed", "Failed to rollback transaction");
        }
    }
    
    public void Dispose()
    {
        _transaction.Dispose();
    }
}