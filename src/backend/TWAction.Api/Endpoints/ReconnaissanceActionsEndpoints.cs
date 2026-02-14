using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.ReconnaissanceActions.Commands;
using TWAction.Application.ReconnaissanceActions.DTOs;
using TWAction.Application.ReconnaissanceActions.Handlers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Queries;
using Wolverine;

namespace TWAction.Api.Endpoints;

public static class ReconnaissanceActionsEndpoints
{
    public static IEndpointRouteBuilder MapReconnaissanceActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/reconnaissance/actions")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("", GetAttackCommands)
            .WithName("GetAttackCommands");

        group.MapPost("", GenerateReconnaissanceActions)
            .WithName("GenerateReconnaissanceActions");

        return app;
    }

    private static async Task<IResult> GetAttackCommands(
        Guid scheduleId,
        HttpContext httpContext,
        GetAttackCommandsHandler handler,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        // Validate schedule ownership
        var ownershipResult = await ValidateScheduleOwnership(scheduleId, httpContext, bus);
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

    private static async Task<IResult> GenerateReconnaissanceActions(
        Guid scheduleId,
        HttpContext httpContext,
        GenerateReconnaissanceActionsHandler handler,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        // Validate schedule ownership
        var ownershipResult = await ValidateScheduleOwnership(scheduleId, httpContext, bus);
        if (ownershipResult is not null)
        {
            return ownershipResult;
        }

        var command = new GenerateReconnaissanceActionsCommand(scheduleId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorMessage = result.Error ?? string.Empty;

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = result.Error });
            }

            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Validates that the schedule exists and belongs to the authenticated user.
    /// </summary>
    /// <returns>IResult error response if validation fails, null if validation passes.</returns>
    private static async Task<IResult?> ValidateScheduleOwnership(
        Guid scheduleId,
        HttpContext httpContext,
        IMessageBus bus)
    {
        var userId = httpContext.User.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var scheduleResult = await bus.InvokeAsync<Result<ScheduleDto>>(new GetScheduleByIdQuery(scheduleId));

        if (scheduleResult.IsFailure)
        {
            return Results.NotFound(new { error = "Schedule not found." });
        }

        if (scheduleResult.Value.UserId != userId.Value)
        {
            return Results.Forbid();
        }

        return null; // Validation passed
    }
}
