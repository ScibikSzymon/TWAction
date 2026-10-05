namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.MainActions.DTOs;
using TWAction.Application.MainActions.Queries;
using Wolverine;

public static class PlayerNobleStatsEndpoints
{
    public static IEndpointRouteBuilder MapPlayerNobleStatsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/player-noble-stats")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("", GetPlayerNobleStats)
            .WithName("GetPlayerNobleStats");

        return app;
    }

    private static async Task<IResult> GetPlayerNobleStats(
        Guid scheduleId,
        IMessageBus bus)
    {
        var query = new GetPlayerNobleStatsQuery(scheduleId);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<PlayerNobleStatsDto>>>(query);

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}
