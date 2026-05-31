using Crm.Api.Application.Messaging;

namespace Crm.Api.Application.ApiResults;

public static class ApiResults
{
    public static IResult ToResult<T>(Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: result.Error.StatusCode);
    }
}