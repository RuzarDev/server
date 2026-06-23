
using Crm.Api.Application.ApiResults;
using Crm.Api.Application.Messaging;
using Crm.Infrastucture;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Features.Tag;

public class CreateTag
{
    public sealed record Command(string Name) : ICommand;
    public class CommandHanlder(ApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var existsTag = await dbContext.Tags.AnyAsync(t=>t.Name == command.Name, cancellationToken: cancellationToken);
            if (existsTag)
            {
                return Result.Failure(Errors.Tag.Exists);
            }
            var tag = new Domain.Entities.Tag{Name = command.Name};
            dbContext.Add(tag);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tags", Handle).RequireAuthorization();
        }

        private static async Task<IResult> Handle(ICommandHandler<Command> commandHandler, [FromBody] Request request, CancellationToken cancellationToken)
        {
            var command = new Command(request.Name);
            var res = await commandHandler.Handle(command,cancellationToken );
            return ApiResults.ToResult(res);
        }
        private sealed record Request(string Name);

    }
}