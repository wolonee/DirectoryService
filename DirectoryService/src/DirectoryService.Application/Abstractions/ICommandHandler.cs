using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Abstractions;

public interface ICommand;

public interface ICommandHandler<TResponse, TCommand>
    where TCommand : ICommand
{
    public Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public Task<UnitResult<Errors>> Handle(TCommand command, CancellationToken cancellationToken = default);
}