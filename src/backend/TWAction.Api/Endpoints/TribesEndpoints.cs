namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TWAction.Api.Filters;
using TWAction.Application.Common;
using TWAction.Application.Tribes.DTOs;
using TWAction.Application.Tribes.Queries;
using TWAction.Domain.Schedules;
using Wolverine;

/// <summary>
/// Request record for retrieving tribes by world.
/// </summary>
/// <param name="World">The world type to retrieve tribes from.</param>
public sealed record GetTribesRequest([FromRoute] WorldType World);

public static class TribesEndpoints
{
    public static IEndpointRouteBuilder MapTribesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/worlds/{world}/tribes").RequireAuthorization();

        group.MapGet("", GetTribes)
            .WithName("GetTribes")
            .AddEndpointFilter<ValidationFilter<GetTribesRequest>>();

        return app;
    }

    private static async Task<IResult> GetTribes(
        [AsParameters] GetTribesRequest request,
        IMessageBus bus)
    {
        var query = new GetTribesQuery(request.World);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<TribeDto>>>(query);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}

