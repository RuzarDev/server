using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact.Note;

public class CreateNote
{
    public sealed record Response(Guid Id, string Text, Guid ContactId, DateTime CreatedAt);

    public sealed record Command(Guid ContactId, string Text) : ICommand<Response>;

    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command,Response>
    {
      
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var validator = Validate(command);
            if (!validator)
            {
                return Result<Response>.Failure(Errors.Validator.Problem);
            }
            var existsContact = await dbContext.Contacts.AnyAsync(c => c.Id == command.ContactId,
                cancellationToken);
            if (!existsContact)
            {
                return Result<Response>.Failure(Errors.Contacts.NotFound);
            }
            var note = new Domain.Entities.Note{Text =  command.Text,ContactId = command.ContactId, CreatedAt =  DateTime.UtcNow};
            dbContext.Notes.Add(note);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(new Response(note.Id, note.Text, note.ContactId, note.CreatedAt));
        }
        public bool Validate(Command command)
        {
            if (string.IsNullOrWhiteSpace(command.Text))
            {
                return false;
            }

            if (Guid.Empty == command.ContactId)
            {
                return false;
            }
            return true;
        }
    }
    public class Endpoint() : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("contacts/note/{id:guid}", Handle).RequireAuthorization();
        }

        public async Task<IResult> Handle(ICommandHandler<Command, Response> commandHandler, [FromRoute] Guid contactId, [FromBody] Request request, CancellationToken cancellationToken)
        {
            var command = new Command(contactId, request.Text);
            var res = await commandHandler.Handle(command, cancellationToken);
            return ApiResults.ToResult(res);
        }
        public sealed record Request( string Text);
    }
}