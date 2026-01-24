namespace TWAction.Application.Common;

/// <summary>
/// Categorizes the type of error that occurred in a failed operation.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// General/unspecified error (default).
    /// </summary>
    None,
    
    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound,
    
    /// <summary>
    /// Validation error (e.g., invalid input format).
    /// </summary>
    Validation,
    
    /// <summary>
    /// Internal server error (e.g., decompression failure, parsing failure, unexpected exceptions).
    /// </summary>
    Internal
}

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public ErrorType ErrorType { get; }
    
    protected Result(bool isSuccess, string error, ErrorType errorType = ErrorType.None)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error.");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error.");
            
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error, ErrorType errorType = ErrorType.None) => new(false, error, errorType);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, string.Empty);
    public static Result<TValue> Failure<TValue>(string error, ErrorType errorType = ErrorType.None) => new(default!, false, error, errorType);
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
    
    internal Result(TValue value, bool isSuccess, string error, ErrorType errorType = ErrorType.None) 
        : base(isSuccess, error, errorType)
    {
        _value = value;
    }

    // Convert to non-generic Result
    public Result AsResult()
    {
        return IsSuccess ? Success() : Failure(Error, ErrorType);
    }
}