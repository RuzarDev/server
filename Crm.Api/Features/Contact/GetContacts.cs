using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Common;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact;

public class GetContacts
{
    public sealed record Response(PaginationResult<Item> Items);
    public sealed record Item(Guid Id,string Name, string Email, string PhoneNumber);

    public record Query(int PageNumber, int PageSize) : IQuery<Response>;
    public class QueryHandler(
        ApplicationDbContext dbContext
        ) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var contacts =  dbContext.Contacts.Select(c=> new Item(c.Id, c.Name, c.Email, c.PhoneNumber));
            var pagination = await PaginationResult<Item>.CreateAsync(contacts,query.PageNumber, query.PageSize);
            return Result<Response>.Success(new Response(pagination));
        }
    }
    public class Endpoint() : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("contacts", Handle).RequireAuthorization();
        }

        private async Task<IResult> Handle(IQueryHandler<Query,Response> queryHandler,[AsParameters] Request request, CancellationToken cancellationToken)
        {
            var query = new Query(request.PageNumber, request.PageSize);
            var result = await queryHandler.Handle(query, cancellationToken);
            return ApiResults.ToResult(result);
        }
        private record Request(int PageNumber, int PageSize);
    }
}