using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Tag;

public class DeleteTagToContact
{
    public sealed record Command(Guid ContactId, Guid TagId ) : ICommand;
    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var validator = Validate(command);
            if (!validator)
            {
                return Result.Failure(Errors.Validator.Problem);
            }
            var contact = await dbContext.Contacts.Include(c => c.Tags)
                .FirstOrDefaultAsync(c => c.Id == command.ContactId && !c.IsDeleted, cancellationToken);
            if (contact == null)
            {
                return Result.Failure(Errors.Contacts.NotFound);
            }
            var existsTagInContact = contact.Tags.FirstOrDefault(t=>t.Id == command.TagId);
            if (existsTagInContact == null)
            {
                return Result.Failure(Errors.Tag.NotFound);
            }
            contact.Tags.Remove(existsTagInContact);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        private static bool Validate(Command command)
        {
            if(command.ContactId == Guid.Empty) return false;
            if(command.TagId == Guid.Empty) return false;
            return true;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("contacts/{contactId:guid}/tags/{tagId:guid}", Handle).RequireAuthorization();
        }

        private async Task<IResult> Handle(ICommandHandler<Command> commandHandler,[FromRoute] Request request, CancellationToken cancellationToken)
        {
            var command = new Command(request.ContactId, request.TagId);
            return ApiResults.ToResult(await commandHandler.Handle(command, cancellationToken));
        }

        private sealed record Request(Guid ContactId, Guid TagId);
    }
}