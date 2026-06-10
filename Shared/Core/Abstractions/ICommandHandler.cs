using CSharpFunctionalExtensions;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Abstractions;

public interface ICommand;

public interface ICommandHandler<TResponse, TCommand>
    where TCommand : ICommand
{
    Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task<UnitResult<Errors>> Handle(TCommand command, CancellationToken cancellationToken = default);
}