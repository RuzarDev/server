
using Crm.Api.Application.Messaging;
using Crm.Domain.Entities;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Authentication;

public class Login
{
    public record Command(string Email, string Password) : ICommand<Response>;
    public record Response(string Token);
    public class CommandHandler(
        ApplicationDbContext dbContext,
        PasswordHasher<User> passwordHasher) : ICommandHandler<Command,Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u=>u.Email == command.Email, cancellationToken);
            if (user is null)
            {
                return Result<Response>.Failure(new Error("User.NotFound","user not found", 404));
            }
            var isSuccess = passwordHasher.VerifyHashedPassword(user, user.Password, command.Password);
            return
                isSuccess == PasswordVerificationResult.Success ?
                    Result<Response>.Success(new Response("token_placeholder")) :
                Result<Response>.Failure(new Error("User.AccessDenied", "password or email wrong", 401));

        }
    }
    public class Endpoint() : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/login", async (
                CommandHandler handler,
                Command command,
                CancellationToken cancellationToken
                ) =>
            {
                var result =  await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.StatusCode(result.Error.StatusCode);
            });
        }
    }
}