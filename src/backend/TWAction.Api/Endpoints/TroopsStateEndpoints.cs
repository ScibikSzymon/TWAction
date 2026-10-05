namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Queries;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Users.Interfaces;
using TWAction.Application.Interfaces;
using TWAction.Domain.Users;
using Wolverine;

public static class TroopsStateEndpoints
{
    public static IEndpointRouteBuilder MapTroopsStateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/troops")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapPost("", UploadTroopsState)
            .WithName("UploadTroopsState");

        group.MapGet("", GetTroopsState)
            .WithName("GetTroopsState");

        group.MapGet("/limit", GetTroopsUploadLimit)
            .WithName("GetTroopsUploadLimit");

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
            if (result.Error.Contains("limit reached", StringComparison.OrdinalIgnoreCase))
                return Results.Conflict(new { error = result.Error });
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

    private static async Task<IResult> GetTroopsUploadLimit(
        Guid scheduleId,
        ICurrentUserAccessor currentUser,
        IScheduleRepository schedules,
        IUserRepository users,
        ISubscriptionPlanLimitsRepository planLimitsRepository,
        ITroopsUploadRepository uploads,
        CancellationToken cancellationToken)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var schedule = await schedules.GetByIdAsync(scheduleId, cancellationToken);
        var canAccessSchedule = schedule is not null
            && (currentUser.IsAdmin || schedule.UserGuid == userId);

        if (!canAccessSchedule)
        {
            return Results.NotFound();
        }

        var owner = await users.GetByIdAsync(schedule!.UserGuid, cancellationToken);
        if (owner is null)
        {
            return Results.NotFound();
        }

        var planLimits = await planLimitsRepository.GetAsync(
            owner.SubscriptionTier,
            cancellationToken);
        var uploadWindow = UserLimits.UploadWindow(planLimits);
        var limit = UserLimits.TroopsUploads(owner, planLimits);
        var windowStart = DateTimeOffset.UtcNow - uploadWindow;
        var recentUploads = await uploads.ListRecentAsync(
            scheduleId,
            windowStart,
            cancellationToken);

        DateTimeOffset? nextAvailableAt = null;
        if (limit is > 0 && recentUploads.Count >= limit.Value)
        {
            var uploadBlockingNextSlot = recentUploads[recentUploads.Count - limit.Value];
            nextAvailableAt = uploadBlockingNextSlot.UploadedAt.Add(uploadWindow);
        }

        return Results.Ok(new
        {
            limit,
            used = recentUploads.Count,
            windowHours = planLimits.TroopsUploadWindowHours,
            nextAvailableAt
        });
    }
}

