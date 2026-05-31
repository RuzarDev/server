namespace Crm.Api.Application.Messaging;

public record Error(string Code, string Message, int StatusCode )
{
    public static readonly Error None = new("", "", 200);
};


