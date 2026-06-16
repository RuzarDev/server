using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.user;

public class GetUser
{
    public record Query(Guid Id) : IQuery<QueryResponse>;
    public record QueryResponse(Guid Id, string Email);
    public class QueryHandler(ApplicationDbContext dbContext) : IQueryHandler<Query,QueryResponse>
    {
        public async Task<Result<QueryResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u=> u.Id == query.Id,cancellationToken);
            if (user is null)
            {
                return Result<QueryResponse>.Failure(new Error("User.NotFound", "user not found", 404));
            }
            return Result<QueryResponse>.Success(new QueryResponse(user.Id, user.Email));
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("user/{id}", Handle).RequireAuthorization();
        }

        private static async Task<IResult> Handle(IQueryHandler<Query,QueryResponse> handler, Guid id, CancellationToken cancellationToken)
        {
            var query = new Query(id);
            var res = await handler.Handle(query,cancellationToken);
            return ApiResults.ToResult(res);
        }
    }
}