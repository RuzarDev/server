using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact.Note;

public class DeleteNote
{
    public sealed record Command(Guid ContactId, Guid NoteId) : ICommand;
    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var validator = Validate(command);
            if (!validator)
            {
                return Result.Failure(Errors.Validator.Problem);
            }
            var existsNote = await dbContext
                .Notes
                .FirstOrDefaultAsync(n=>n.Id == command.NoteId && n.ContactId == command.ContactId, cancellationToken);
            if (existsNote is null) 
            {
                return Result.Failure(Errors.Note.NotFound);
            }
            dbContext.Notes.Remove(existsNote);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        private static bool Validate(Command command)
        {
            return command.NoteId != Guid.Empty &&  command.ContactId != Guid.Empty;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("contacts/{contactId:guid}/notes/{noteId:guid}", Handle).RequireAuthorization();
        }

        private static async Task<IResult> Handle(ICommandHandler<Command> commandHandler, [FromRoute] Request request, CancellationToken cancellationToken)
        {
            var command = new Command(request.ContactId, request.NoteId);
            return ApiResults.ToResult( await commandHandler.Handle(command, cancellationToken));
        }
        private sealed record Request(Guid ContactId, Guid NoteId);
    }
}
