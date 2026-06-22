
using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact;

public class UpdateContact
{
    public sealed record Command(Guid Id, string Name, string Email, string PhoneNumber) : ICommand;
    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var existEmail = await dbContext.Contacts.AnyAsync(c=>c.Email == command.Email && c.Id != command.Id, cancellationToken);
            if (existEmail)
            {
                return Result.Failure(Errors.Contacts.Exists);
            }
            var contact = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == command.Id && x.IsDeleted == false,  cancellationToken);
            if (contact is null)
            {
                return Result.Failure(Errors.Contacts.NotFound);
            }
            contact.Name = command.Name;
            contact.Email = command.Email;
            contact.PhoneNumber = command.PhoneNumber;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

    public class Endpoint() : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("contacts/{id:guid}", Handle).RequireAuthorization();
        }

        private async Task<IResult> Handle(ICommandHandler<Command> commandHandler, [FromRoute] Guid id, [FromBody] Request request, CancellationToken cancellationToken)
        {
            var command = new Command(id, request.Name, request.Email, request.PhoneNumber);
            var res= await commandHandler.Handle(command, cancellationToken);
            return ApiResults.ToResult(res);
        }
        private sealed record Request(string Name, string Email, string PhoneNumber);
    }
}