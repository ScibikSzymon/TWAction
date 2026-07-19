namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Queries;
using Wolverine;

public static class NobleBudgetEndpoints
{
    public static IEndpointRouteBuilder MapNobleBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/noble-budgets")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("", GetNobleBudgets)
            .WithName("GetNobleBudgets");

        group.MapPost("", SaveNobleBudgets)
            .WithName("SaveNobleBudgets");

        return app;
    }

    private static async Task<IResult> GetNobleBudgets(
        Guid scheduleId,
        IMessageBus bus)
    {
        var query = new GetNobleBudgetsQuery(scheduleId);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<NobleBudgetDto>>>(query);

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SaveNobleBudgets(
        Guid scheduleId,
        SaveNobleBudgetsRequest request,
        IMessageBus bus)
    {
        var command = new SaveNobleBudgetsCommand(scheduleId, request.PlayerBudgets);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<NobleBudgetDto>>>(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = result.Error });
            }

            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}
