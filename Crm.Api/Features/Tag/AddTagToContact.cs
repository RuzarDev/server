using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Tag;

public class AddTagToContact
{
    public sealed record Command(Guid ContactId, Guid TagId) : ICommand;
    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var validate = Validate(command);
            if (!validate)
            {
                return Result.Failure(Errors.Validator.Problem);
            }
            var contact = await dbContext.Contacts                                                                                                                                                                                             
                .Include(c => c.Tags)                                                                                                                                                                                                          
                .FirstOrDefaultAsync(x => x.Id == command.ContactId && !x.IsDeleted, cancellationToken);
            if (contact == null)
            {
                return Result.Failure(Errors.Contacts.NotFound);
            }
            var tag = await dbContext.Tags.FirstOrDefaultAsync(x => x.Id == command.TagId, cancellationToken: cancellationToken);
            if (tag == null)
            {
                return Result.Failure(Errors.Tag.NotFound);
            }
            if (contact.Tags.Any(t => t.Id == command.TagId))
            {
                return Result.Failure(Errors.Tag.Exists);
            }
            contact.Tags.Add(tag);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        private static bool Validate(Command command)
        {
            if (command.ContactId == Guid.Empty)
            {
                return false;
            }

            if (command.TagId == Guid.Empty)
            {
                return false;
            }
            return true;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("contacts/{contactId:guid}/tags/{tagId:guid}", Handle).RequireAuthorization();
        }

        private  static async Task<IResult> Handle(ICommandHandler<Command> commandHandler, [FromRoute] Request request,
            CancellationToken cancellationToken)
        {
            var command = new Command(request.ContactId, request.TagId);
            var res = await commandHandler.Handle(command, cancellationToken);
            return ApiResults.ToResult(res);
        }
        private sealed record Request(Guid ContactId, Guid TagId);
    }
}