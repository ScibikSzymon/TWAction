namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Queries;
using Wolverine;

public static class TroopsStateEndpoints
{
    public static IEndpointRouteBuilder MapTroopsStateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/troops").RequireAuthorization();

        group.MapPost("", UploadTroopsState)
            .WithName("UploadTroopsState");

        group.MapGet("", GetTroopsState)
            .WithName("GetTroopsState");

        return app;
    }

    private static async Task<IResult> UploadTroopsState(
        Guid scheduleId,
        UploadTroopsStateRequest request,
        IMessageBus bus)
    {
        var command = new UploadTroopsStateCommand(scheduleId, request.RawData);

        var result = await bus.InvokeAsync<Result<TroopsStateDto>>(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = result.Error });
            }

            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Created($"/schedules/{scheduleId}/troops", result.Value);
    }

    private static async Task<IResult> GetTroopsState(
        Guid scheduleId,
        IMessageBus bus)
    {
        var query = new GetTroopsStateQuery(scheduleId);

        var result = await bus.InvokeAsync<Result<TroopsStateDto>>(query);

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}

