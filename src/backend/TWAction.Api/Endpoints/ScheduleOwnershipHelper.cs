using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Queries;
using Wolverine;

namespace TWAction.Api.Endpoints;

/// <summary>
/// Shared helper for validating schedule ownership across endpoint classes.
/// </summary>
internal static class ScheduleOwnershipHelper
{
    /// <summary>
    /// Validates that the schedule exists and belongs to the authenticated user.
    /// </summary>
    /// <returns>IResult error response if validation fails, null if validation passes.</returns>
    public static async Task<IResult?> ValidateOwnershipAsync(
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
