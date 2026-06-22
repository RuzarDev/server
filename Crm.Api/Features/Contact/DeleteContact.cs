
using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact;

public class DeleteContact
{
    public sealed record Command(Guid Id) : ICommand;
    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var existContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == command.Id,cancellationToken);
            if (existContact is null)
            {
                return Result.Failure(Errors.Contacts.NotFound);
            }
            existContact.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("contacts/{id:guid}", Handle).RequireAuthorization();
        }

        private async Task<IResult> Handle(ICommandHandler<Command> commandHandler,[FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new Command(id);
            var res = await commandHandler.Handle(command, cancellationToken);
            return ApiResults.ToResult(res);
        }
    }
}