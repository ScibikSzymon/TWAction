namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.DTOs;
using Wolverine;

public static class TroopsStateEndpoints
{
    public static IEndpointRouteBuilder MapTroopsStateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/troops");

        group.MapPost("", UploadTroopsState)
            .WithName("UploadTroopsState");

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
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Created($"/schedules/{scheduleId}/troops", result.Value);
    }
}
