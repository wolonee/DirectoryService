using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Abstractions;

public interface ICommand;

public interface ICommandHandler<TResponse, TCommand>
    where TCommand : ICommand
{
    public Task<Result<TResponse, Failure>> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public Task<UnitResult<Failure>> Handle(TCommand command, CancellationToken cancellationToken = default);
}