using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Shared;

namespace DirectoryService.Application.Abstractions;

public interface IQuery;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Errors>> Handle(TQuery query, CancellationToken cancellationToken = default);
}