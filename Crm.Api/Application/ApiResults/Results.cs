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
    public static IResult ToResult(Result result)                                                                                                                                                                                      
    {                                                                                                                                                                                                                                  
        return result.IsSuccess                                                                                                                                                                                                        
            ? Results.NoContent()                                                                                                                                                                                                      
            : Results.Problem(detail: result.Error.Message, statusCode: result.Error.StatusCode);                                                                                                                                      
    }   
    public static IResult ToResultCreated<T>(Result<T> result,string url)
    {
        return result.IsSuccess
            ? Results.Created<T>(url,result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: result.Error.StatusCode);
    }
}