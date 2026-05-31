
using Crm.Api.Features.Authentication;

namespace Crm.Api.Application.Messaging;

public interface ICommandHandler<TCommand> where TCommand: ICommand
{
    public Task<Result> Handle(TCommand command,CancellationToken cancellationToken);
}

public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    public Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}