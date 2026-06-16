
using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact;

public class GetContact
{
    public sealed record Response(Guid Id,string Name, string Email, string PhoneNumber);
    public sealed record Command(Guid Id) : ICommand<Response>;
    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command,Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var contact = await dbContext.Contacts.FirstOrDefaultAsync(c=>c.Id == command.Id, cancellationToken);
            if (contact is null)
            {
               return Result<Response>.Failure(Errors.Contacts.NotFound);
            }
            var response = new Response(contact.Id, contact.Name, contact.Email, contact.PhoneNumber);
            return Result<Response>.Success(response);
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("contacts/{id:guid}", Handle);
        }

        private async Task<IResult> Handle(ICommandHandler<Command, Response> commandHandler,[FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new Command(id);
            var res =  await commandHandler.Handle(command, cancellationToken);
            return ApiResults.ToResult(res);
        }
    }
}