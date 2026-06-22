using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Common;
using Crm.Api.Application.Messaging;
using Crm.Api.Application.Sorting;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;

namespace Crm.Api.Features.user;

public class GetUsers
{
    public record Query(int PageIndex, int PageSize,string? Sort): IQuery<Response>;

    public record User(Guid Id, string Email);
    public record Response(PaginationResult<User> Users);
 public static class Mappings                                                                                                                                                                                                       
    {                                                                                                                                                                                                                                  
      public static readonly SortMappingDefinition<Response, Domain.Entities.User> SortMapping = new()                                                                                                                               
      {                                                                                                                                                                                                                              
          Mappings =                                                                                                                                                                                                                 
          [                                                                                                                                                                                                                          
              new SortMapping("email", "Email",false),                                                                                                                                                                                     
              new SortMapping("id", "Id",false),                                                                                                                                                                                           
          ]                                                                                                                                                                                                                          
      };                                                                                                                                                                                                                             
    }      
    public class QueryHandler(ApplicationDbContext dbContext, SortMappingProvider provider ) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var users = dbContext.Users
                .Sort(query.Sort,provider.GetMappings<Response,Domain.Entities.User>())
                .Select(u=>new User(u.Id,u.Email));
            var pagination = await PaginationResult<User>.CreateAsync(users,query.PageIndex, query.PageSize);
            return Result<Response>.Success(new Response(pagination));
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("users", Handle).RequireAuthorization();
        }

        private  static async Task<IResult> Handle( IQueryHandler<Query,Response> queryHandler ,[AsParameters] Request request , CancellationToken cancellationToken)
        {
                var query = new Query(request.PageIndex, request.PageSize,request.Sort);
                var users = await queryHandler.Handle(query, cancellationToken);
                return ApiResults.ToResult(users);
        }
        private record Request(int PageIndex, int PageSize,string? Sort);
    }
}