using FluentValidation.Results;

namespace TWAction.Application.Common.Validation;

/// <summary>
/// Extensions for converting FluentValidation results to application Result pattern.
/// </summary>
public static class ValidationResultExtensions
{
    public static Result ToResult(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return Result.Success();
        }

        var errorMessage = FormatErrors(validationResult.Errors);
        return Result.Failure(errorMessage);
    }

    public static Result<T> ToResult<T>(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            throw new InvalidOperationException(
                "Cannot convert a valid ValidationResult to a failure Result<T>. " +
                "Use this method only when validation has failed.");
        }

        var errorMessage = FormatErrors(validationResult.Errors);
        return Result.Failure<T>(errorMessage);
    }

    /// <summary>
    /// Formats validation errors into a single error message string.
    /// </summary>
    /// <param name="errors">The validation failures.</param>
    /// <returns>A formatted error message string.</returns>
    private static string FormatErrors(IEnumerable<ValidationFailure> errors)
    {
        var errorMessages = errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
        return string.Join("; ", errorMessages);
    }
}
