namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Application.Common;
using TWAction.Application.Tribes.DTOs;
using TWAction.Application.Tribes.Queries;
using TWAction.Domain.Schedules;
using Wolverine;

public static class TribesEndpoints
{
    public static IEndpointRouteBuilder MapTribesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/worlds/{world}/tribes");

        group.MapGet("", GetTribes)
            .WithName("GetTribes");

        return app;
    }

    private static async Task<IResult> GetTribes(
        WorldType world,
        IMessageBus bus)
    {
        var query = new GetTribesQuery(world);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<TribeDto>>>(query);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

}

