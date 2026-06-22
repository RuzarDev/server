using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.User;

public  class SoftDeleteUser
{
    public record Command(Guid UserId) : ICommand;

    public class CommandHandler(ApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u=>u.Id == command.UserId,cancellationToken);
            if (user is null)
            {
                return Result.Failure(new Error("User.NotFound", "User not found",404));
            }
            user.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("users/{id}", Handler).RequireAuthorization();
        }

        private static async  Task<IResult> Handler([FromServices]ICommandHandler<Command> handler,[FromQuery] Guid id, CancellationToken cancellationToken)
        {
            var command = new Command(id);
            var result = await handler.Handle(command, cancellationToken);
            return ApiResults.ToResult(result);
        }
    }
}