namespace ChatSystem.ErrorHandling;
public class Result
{
    public bool IsSuccess{get;}
    public int StatusCode{get;}
    public string? Error{get;}
    protected Result(bool isSuccess, int statusCode, string? error)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Error = error;
    }
    public static Result Success(int statusCode = 200) => new(true, statusCode, null);
    public static Result Failure(string error, int statusCode) => new(false, statusCode, error); 
}
public class Result<T> : Result
{
    public T? Value{get;}
    public Result(T? value, bool isSuccess, int statusCode, string? error) : base(isSuccess, statusCode, error)
    {
        Value = value;
    }
    public static Result<T> Success(T value) => new(value, true, 200, null);
    public static new Result<T> Failure(string error, int statusCode) => new(default, false, statusCode, error); 
}