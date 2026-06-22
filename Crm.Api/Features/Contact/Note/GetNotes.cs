using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact.Note;

public class GetNotes
{
    public sealed record Response(Guid Id, Guid ContactId, string Text, DateTime CreatedAt);

    public sealed record Command(Guid Id, Guid ContactId) : ICommand<Response>;
    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command,Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var validate = Validate(command);
            if (!validate)
            {
                return Result<Response>.Failure(Errors.Validator.Problem);
            }
            var existsContact = await dbContext.Contacts.AnyAsync(x => x.Id == command.ContactId, cancellationToken);
            if (!existsContact)
            {
                return Result<Response>.Failure(Errors.Contacts.NotFound);
            }
            var note = await dbContext.Notes.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            throw new NotImplementedException();
        }

        public bool Validate(Command command)
        {
            if (command.ContactId == Guid.Empty)
            {
                return false;
            }

            if (command.Id == Guid.Empty)
            {
                return false;
            }
            return true;
        }
    }
}