
using Crm.Api.Application.Messaging;
using Crm.Domain.Entities;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Authentication;

public static class Register
{
    public sealed record Command(string Email, string Password) : ICommand<Response>;
    public sealed record Response(string Email);
    public  class CommandHandler(
        ApplicationDbContext dbContext,
        PasswordHasher<User> passwordHasher) : ICommandHandler<Command,Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email, cancellationToken);
            if (user is not null)
            {
                return Result<Response>.Failure(
                    new Error(
                        "User.AlreadyExists",
                        "User already exists",
                        409
                        ));
            }
            var newUser = new User {Email = command.Email};
            var password = passwordHasher.HashPassword( newUser, command.Password); 
            newUser.Password = password;
            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(new Response(newUser.Email));
        }
    }
    public  class Endpoint(): IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/register",
                async (ICommandHandler<Command,Response> handler,Request request, CancellationToken cancellationToken) =>
                {
                    var command = new Command(request.Email, request.Password);
                    var result = await handler.Handle(command, cancellationToken);
                    return  result.IsSuccess ? Results.Ok(result.Value) : Results.StatusCode(result.Error.StatusCode);
                });
        }
        public record Request(string Email, string Password);
    }
}