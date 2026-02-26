using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

        group.MapGet("", GetAttackCommands)
            .WithName("GetAttackCommands")
            .Produces<IReadOnlyList<AttackCommandResponseDto>>();

        return app;
    }

    private static async Task<IResult> GetAttackCommands(
        Guid scheduleId,
        HttpContext httpContext,
        GetAttackCommandsHandler handler,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var ownershipResult = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipResult is not null)
        {
            return ownershipResult;
        }

        var query = new GetAttackCommandsQuery(scheduleId);
        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}
