using TWAction.Api.Extensions;
using TWAction.Application.AttackCommands.DTOs;
using TWAction.Application.AttackCommands.Handlers;
using TWAction.Application.AttackCommands.Queries;
using Wolverine;

namespace TWAction.Api.Endpoints;

public static class AttackCommandsEndpoints
{
    public static IEndpointRouteBuilder MapAttackCommandsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/attack-commands")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("summary", GetAttackCommandsSummary)
            .WithName("GetAttackCommandsSummary")
            .Produces<AttackCommandsSummaryDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("send-to-plemiona-rozpiski", SendToPlemionaRozpiski)
            .WithName("SendToPlemionaRozpiski")
            .Produces<SendToPlemionaRozpiskiResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAttackCommandsSummary(
        Guid scheduleId,
        HttpContext httpContext,
        GetAttackCommandsSummaryHandler handler,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var ownershipResult = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipResult is not null)
        {
            return ownershipResult;
        }

        var query = new GetAttackCommandsSummaryQuery(scheduleId);
        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound();
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SendToPlemionaRozpiski(
        Guid scheduleId,
        [AsParameters] SendToPlemionaRozpiskiRequest request,
        HttpContext httpContext,
        SendToPlemionaRozpiskiHandler handler,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var ownershipResult = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipResult is not null)
        {
            return ownershipResult;
        }

        var command = new SendToPlemionaRozpiskiCommand(scheduleId, request.ForceOverwrite);
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}

public sealed record SendToPlemionaRozpiskiRequest(bool ForceOverwrite = false);
