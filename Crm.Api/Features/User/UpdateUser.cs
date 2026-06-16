
using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.user;

public class UpdateUser
{
    public record Command(Guid Id, string Email) : ICommand;
    public class CommandHandler(
        ApplicationDbContext dbContext
        ) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var validateResult =  Validate(command);
            if (validateResult is false)
            {
                return Result.Failure(new Error("Validation.Problem", "One of validations didn't pass",400));
            }
            var email = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email, cancellationToken: cancellationToken);
            if (email is not null)
            {
                return Result.Failure(new Error("User.AlreadyExists", "User already exists",409));
            }
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);
            if (user is null)
            {
                return Result.Failure(new Error("User.NotFound", "User not found",404));
            }
            user.Email = command.Email;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public bool Validate(Command command)
        {
            if (string.IsNullOrEmpty(command.Email) || !command.Email.Contains("@"))
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
    public  class Endpoint() : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("users/{id:guid}",Handler).RequireAuthorization();
        }
        private async Task<IResult> Handler(ICommandHandler<Command> commandHandler, [FromRoute] Guid id, [FromBody] Request request, CancellationToken cancellationToken)
         {
            var command = new Command(id,request.Email);
            var res = await commandHandler.Handle(command, cancellationToken);
            return ApiResults.ToResult(res);
        }

        private  record Request(string Email);
            
    }
}