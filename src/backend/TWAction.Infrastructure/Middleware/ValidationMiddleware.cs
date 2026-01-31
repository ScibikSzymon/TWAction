using FluentValidation;
using TWAction.Application.Common;
using Wolverine;

namespace TWAction.Infrastructure.Middleware;

/// <summary>
/// Wolverine middleware that executes FluentValidation validators before handlers.
/// Returns Result pattern instead of throwing exceptions when validation fails.
/// </summary>
public static class ValidationMiddleware
{
    /// <summary>
    /// Validates the message before handler execution for handlers returning Result{T}.
    /// If validation fails, short-circuits and returns a failure Result.
    /// </summary>
    /// <typeparam name="TMessage">The message type being validated.</typeparam>
    /// <typeparam name="TResponse">The response type wrapped in Result.</typeparam>
    /// <param name="message">The message to validate.</param>
    /// <param name="validators">All registered validators for this message type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A tuple containing:
    /// - HandlerContinuation: Continue if valid, Stop if validation failed
    /// - Result{TResponse}?: The failure result if validation failed, null otherwise
    /// </returns>
    public static async Task<(HandlerContinuation, Result<TResponse>?)> BeforeAsync<TMessage, TResponse>(
        TMessage message,
        IEnumerable<IValidator<TMessage>> validators,
        CancellationToken cancellationToken)
    {
        var validatorList = validators.ToList();

        if (validatorList.Count == 0)
        {
            return (HandlerContinuation.Continue, null);
        }

        var context = new ValidationContext<TMessage>(message);
        var validationTasks = validatorList.Select(v => v.ValidateAsync(context, cancellationToken));
        var results = await Task.WhenAll(validationTasks);

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return (HandlerContinuation.Continue, null);
        }

        var errorMessages = failures.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
        var combinedError = string.Join("; ", errorMessages);
        var failureResult = Result.Failure<TResponse>(combinedError);

        return (HandlerContinuation.Stop, failureResult);
    }

    /// <summary>
    /// Validates the message before handler execution for handlers returning non-generic Result.
    /// If validation fails, short-circuits and returns a failure Result.
    /// </summary>
    /// <typeparam name="TMessage">The message type being validated.</typeparam>
    /// <param name="message">The message to validate.</param>
    /// <param name="validators">All registered validators for this message type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A tuple containing:
    /// - HandlerContinuation: Continue if valid, Stop if validation failed
    /// - Result?: The failure result if validation failed, null otherwise
    /// </returns>
    public static async Task<(HandlerContinuation, Result?)> BeforeAsync<TMessage>(
        TMessage message,
        IEnumerable<IValidator<TMessage>> validators,
        CancellationToken cancellationToken)
    {
        var validatorList = validators.ToList();

        if (validatorList.Count == 0)
        {
            return (HandlerContinuation.Continue, null);
        }

        var context = new ValidationContext<TMessage>(message);
        var validationTasks = validatorList.Select(v => v.ValidateAsync(context, cancellationToken));
        var results = await Task.WhenAll(validationTasks);

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return (HandlerContinuation.Continue, null);
        }

        var errorMessages = failures.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
        var combinedError = string.Join("; ", errorMessages);
        var failureResult = Result.Failure(combinedError);

        return (HandlerContinuation.Stop, failureResult);
    }
}
