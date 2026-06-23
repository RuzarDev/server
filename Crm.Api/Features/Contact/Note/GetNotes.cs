using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Common;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact.Note;

public class GetNotes
{
    public sealed record Response(PaginationResult<Note> Data);

    public sealed record Note(Guid Id, Guid ContactId, string Text, DateTime CreatedAt);

    public sealed record Query(Guid ContactId, int PageIndex, int PageSize) : IQuery<Response>;
    public class CommandHandler(ApplicationDbContext dbContext) : IQueryHandler<Query,Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var validate = Validate(query);
            if (!validate)
            {
                return Result<Response>.Failure(Errors.Validator.Problem);
            }
            var existsContact = await dbContext.Contacts.AnyAsync(x =>!x.IsDeleted && x.Id == query.ContactId, cancellationToken);
            if (!existsContact)
            {
                return Result<Response>.Failure(Errors.Contacts.NotFound);
            }

            var note = dbContext.Notes.Select(n=>new Note(n.Id,n.ContactId, n.Text, n.CreatedAt)).Where(n=>n.ContactId == query.ContactId);
            var pagination =  await PaginationResult<Note>.CreateAsync(note, query.PageIndex, query.PageSize,cancellationToken);
            return Result<Response>.Success(new Response(pagination));
        }

        private static bool Validate(Query command)
        {
            return command.ContactId != Guid.Empty;
        }
    }
    public class Endpoint() : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("contacts/{contactId:guid}/notes",Handle);
        }

        private static async Task<IResult> Handle(IQueryHandler<Query, Response> queryHandler, [FromRoute] Guid contactId, [AsParameters] Request request, CancellationToken cancellationToken)
        {
            var query = new Query(contactId,request.Page,request.PageSize);
            var result = await queryHandler.Handle(query, cancellationToken);
            return ApiResults.ToResult(result);
        }

        private sealed record Request(int Page, int PageSize);
    }
}