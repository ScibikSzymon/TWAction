namespace TWAction.Api.Filters;

using FluentValidation;

/// <summary>
/// An endpoint filter that automatically validates request objects using FluentValidation.
/// </summary>
/// <typeparam name="T">The type of the request object to validate.</typeparam>
/// <example>
/// <code>
/// group.MapPost("", CreateSchedule)
///     .AddEndpointFilter&lt;ValidationFilter&lt;CreateScheduleRequest&gt;&gt;();
/// </code>
/// </example>
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    /// <summary>
    /// Invokes the validation filter, validating the request object before passing control to the next filter.
    /// </summary>
    /// <param name="context">The endpoint filter invocation context.</param>
    /// <param name="next">The next filter delegate in the pipeline.</param>
    /// <returns>
    /// A validation problem result if validation fails; otherwise, the result of the next filter.
    /// </returns>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is null)
        {
            return await next(context);
        }

        var entity = context.Arguments.OfType<T>().FirstOrDefault();

        if (entity is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(entity);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
