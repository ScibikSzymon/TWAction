namespace TWAction.Application.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error.");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error.");
            
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, string.Empty);
    public static Result<TValue> Failure<TValue>(string error) => new(default!, false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;
    
    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access value of a failed result.");
    
    internal Result(TValue value, bool isSuccess, string error) 
        : base(isSuccess, error)
    {
        _value = value;
    }

    // Convert to non-generic Result
    public Result AsResult()
    {
        return IsSuccess ? Success() : Failure(Error);
    }
}