namespace Crm.Api.Application.Messaging;

public interface IQueryHandler<TQuery> where TQuery : IQuery
{
    public Task<Result> Handle(TQuery query, CancellationToken cancellationToken); 
};

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    public Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
};
