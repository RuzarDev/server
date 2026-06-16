using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Contact;

public class CreateContact
{
    public sealed record Response(Guid Id, string Name, string Email, string PhoneNumber);

    public sealed record Command(string Name, string Email, string PhoneNumber) : ICommand<Response>;
    public class CommandHandler(
        ApplicationDbContext dbContext
        ) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existContact = await dbContext.Contacts.AnyAsync(c=>c.Email==command.Email, cancellationToken);
            if (existContact)
            {
                return Result<Response>.Failure(Errors.Contacts.Exists);
            }
            var contact = new Domain.Entities.Contact{Email = command.Email, Name = command.Name,PhoneNumber = command.PhoneNumber};
            dbContext.Contacts.Add(contact);
            await  dbContext.SaveChangesAsync(cancellationToken);
            var response = new Response(contact.Id, contact.Name, contact.Email, contact.PhoneNumber);
            return Result<Response>.Success(response);
        }
    }
    public class Endpoint() : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("contacts", Handle).RequireAuthorization();
        }
        private async Task<IResult> Handle(ICommandHandler<Command,Response> commandHandler,[FromBody] Request request, CancellationToken cancellationToken)
        {
          var command = new Command(request.Name, request.Email, request.PhoneNumber);  
          var res = await commandHandler.Handle(command, cancellationToken);
          return ApiResults.ToResultCreated<Response>(res,$"contacts/{res.Value!.Id}");
        }
        private sealed record Request(string Name, string Email, string PhoneNumber);
    }
}