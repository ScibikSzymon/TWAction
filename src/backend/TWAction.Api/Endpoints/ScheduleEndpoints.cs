namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Api.Filters;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Queries;
using TWAction.Domain.Schedules;
using Wolverine;

public static class ScheduleEndpoints
{
    public static IEndpointRouteBuilder MapScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("", GetSchedulesForCurrentUser)
            .WithName("GetSchedulesForCurrentUser");

        group.MapGet("/admin/{userId}", GetSchedulesByUserAsAdmin)
            .WithName("GetSchedulesByUserAsAdmin")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/{scheduleId}", GetScheduleById)
            .WithName("GetScheduleById");

        group.MapPost("", CreateSchedule)
            .WithName("CreateSchedule")
            .AddEndpointFilter<ValidationFilter<CreateScheduleRequest>>();

        group.MapPut("/{scheduleId}", UpdateSchedule)
            .WithName("UpdateSchedule")
            .AddEndpointFilter<ValidationFilter<UpdateScheduleRequest>>();

        group.MapDelete("/{scheduleId}", DeleteSchedule)
            .WithName("DeleteSchedule");

        return app;
    }

    private static async Task<IResult> GetSchedulesForCurrentUser(
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await bus.InvokeAsync<Result<IEnumerable<ScheduleDto>>>(new GetAllSchedulesQuery(userId));
        
        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetSchedulesByUserAsAdmin(
        Guid userId,
        IMessageBus bus)
    {
        var result = await bus.InvokeAsync<Result<IEnumerable<ScheduleDto>>>(new GetAllSchedulesQuery(userId));

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetScheduleById(
        Guid scheduleId,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await bus.InvokeAsync<Result<ScheduleDto>>(new GetScheduleByIdQuery(scheduleId));

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        if (!currentUser.IsAdmin && result.Value.UserId != userId)
        {
            return Results.NotFound(new { error = "Schedule not found for specified user." });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateSchedule(
        CreateScheduleRequest request,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new CreateScheduleCommand(
            userId,
            request.Name,
            request.World,
            request.ScheduleType,
            request.EnemyTribalWarsIds ?? []
        );


        var result = await bus.InvokeAsync<Result<ScheduleDto>>(command);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Created($"/schedules/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateSchedule(
        Guid scheduleId,
        UpdateScheduleRequest request,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var scheduleResult = await bus.InvokeAsync<Result<ScheduleDto>>(new GetScheduleByIdQuery(scheduleId));

        if (scheduleResult.IsFailure)
        {
            return Results.NotFound(new { error = scheduleResult.Error });
        }

        if (!currentUser.IsAdmin && scheduleResult.Value.UserId != userId)
        {
            return Results.NotFound(new { error = "Schedule not found for specified user." });
        }

        var command = new UpdateScheduleCommand(
            scheduleId,
            request.Name,
            request.World,
            request.ScheduleType,
            request.EnemyTribalWarsIds
        );

        var result = await bus.InvokeAsync<Result<ScheduleDto>>(command);

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteSchedule(
        Guid scheduleId,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var scheduleResult = await bus.InvokeAsync<Result<ScheduleDto>>(new GetScheduleByIdQuery(scheduleId));

        if (scheduleResult.IsFailure)
        {
            return Results.NotFound(new { error = scheduleResult.Error });
        }

        if (!currentUser.IsAdmin && scheduleResult.Value.UserId != userId)
        {
            return Results.NotFound(new { error = "Schedule not found for specified user." });
        }

        var result = await bus.InvokeAsync<Result>(new DeleteScheduleCommand(scheduleId));

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }


        return Results.NoContent();
    }
}

public sealed record CreateScheduleRequest(
    string Name,
    WorldType World,
    ScheduleType ScheduleType,
    IReadOnlyList<int>? EnemyTribalWarsIds = null
);

public sealed record UpdateScheduleRequest(
    string Name,
    WorldType World,
    ScheduleType ScheduleType,
    IReadOnlyList<int>? EnemyTribalWarsIds = null
);




