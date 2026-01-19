namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Queries;
using Wolverine;

public static class ScheduleEndpoints
{
    public static IEndpointRouteBuilder MapScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules");

        group.MapGet("/{userId}", GetSchedulesByUser)
            .WithName("GetSchedulesByUser");

        group.MapGet("/{userId}/{scheduleId}", GetScheduleById)
            .WithName("GetScheduleById");

        group.MapPost("", CreateSchedule)
            .WithName("CreateSchedule");

        group.MapPut("/{scheduleId}", UpdateSchedule)
            .WithName("UpdateSchedule");

        group.MapDelete("/{scheduleId}", DeleteSchedule)
            .WithName("DeleteSchedule");

        return app;
    }

    private static async Task<IResult> GetSchedulesByUser(
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
        Guid userId,
        Guid scheduleId,
        IMessageBus bus)
    {
        var result = await bus.InvokeAsync<Result<ScheduleDto>>(new GetScheduleByIdQuery(scheduleId));

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateSchedule(
        CreateScheduleRequest request,
        IMessageBus bus)
    {
        var command = new CreateScheduleCommand(
            request.UserId,
            request.Name,
            request.World,
            request.ScheduleType
        );

        var result = await bus.InvokeAsync<Result<ScheduleDto>>(command);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Created($"/schedules/{result.Value.UserId}/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateSchedule(
        Guid scheduleId,
        UpdateScheduleRequest request,
        IMessageBus bus)
    {
        var command = new UpdateScheduleCommand(
            scheduleId,
            request.Name,
            request.World,
            request.ScheduleType
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
        IMessageBus bus)
    {
        var result = await bus.InvokeAsync<Result>(new DeleteScheduleCommand(scheduleId));

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.NoContent();
    }
}

public sealed record CreateScheduleRequest(
    Guid UserId,
    string Name,
    string World,
    string ScheduleType
);

public sealed record UpdateScheduleRequest(
    string Name,
    string World,
    string ScheduleType
);
