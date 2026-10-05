namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.MainActions.Commands;
using Wolverine;

public static class MainActionGeneratorEndpoints
{
    public static IEndpointRouteBuilder MapMainActionGeneratorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/main-action")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapPost("generate", GenerateMainActions)
            .WithName("GenerateMainActions");

        return app;
    }

    private static async Task<IResult> GenerateMainActions(
        Guid scheduleId,
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ownershipError = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipError is not null)
        {
            return ownershipError;
        }

        var command = new GenerateMainActionsCommand(scheduleId);
        var result = await bus.InvokeAsync<Result<GenerateMainActionsResponse>>(command, cancellationToken);

        if (result.IsFailure)
        {
            var error = result.Error ?? string.Empty;
            if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = result.Error });
            }

            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}
