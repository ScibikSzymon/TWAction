using FluentValidation;

namespace TWAction.Application.Common;

/// <summary>
/// Shared FluentValidation helper for Wolverine handlers.
/// Call <see cref="ValidateAsync{TQuery,TResult}"/> at the start of each handler's
/// <c>Handle</c> method to run FluentValidation and short-circuit with a <see cref="Result"/>
/// failure when the query/command is invalid.
/// </summary>
public static class FluentValidationBefore
{
    /// <summary>
    /// Runs <see cref="IValidator{T}"/> and returns a <see cref="Result{TResult}"/> failure
    /// when validation fails, or <c>null</c> when the query is valid.
    /// </summary>
    public static async Task<Result<TResult>?> ValidateAsync<TQuery, TResult>(
        IValidator<TQuery> validator,
        TQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TResult>(errors);
        }

        return null;
    }

    /// <summary>
    /// Runs <see cref="IValidator{T}"/> and returns a non-generic <see cref="Result"/> failure
    /// when validation fails, or <c>null</c> when the command is valid.
    /// </summary>
    public static async Task<Result?> ValidateAsync<TQuery>(
        IValidator<TQuery> validator,
        TQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        return null;
    }
}
