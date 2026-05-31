
namespace Crm.Api.Application.Messaging;
public class Result<T> : Result
{
    public T? Value { get; init; }
    private Result(bool isSuccess,Error error, T? value) : base(isSuccess, error) 
    {
        Value = value;
    }
    public static Result<T> Success(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        return new Result<T>(true, Error.None, value);
    }

    public new static Result<T> Failure(Error error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }
        return new Result<T>(false, error, default);
    }
}