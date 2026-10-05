namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.TargetGroups.Commands;
using TWAction.Application.TargetGroups.DTOs;
using TWAction.Application.TargetGroups.Queries;
using Wolverine;

public static class TargetGroupEndpoints
{
    public static IEndpointRouteBuilder MapTargetGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/target-groups")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("", GetTargetGroups).WithName("GetTargetGroups");
        group.MapGet("/{groupId}", GetTargetGroupById).WithName("GetTargetGroupById");
        group.MapPost("", CreateTargetGroup).WithName("CreateTargetGroup");
        group.MapPut("/{groupId}", UpdateTargetGroup).WithName("UpdateTargetGroup");
        group.MapDelete("/{groupId}", DeleteTargetGroup).WithName("DeleteTargetGroup");

        return app;
    }

    private static async Task<IResult> GetTargetGroups(
        Guid scheduleId,
        IMessageBus bus,
        HttpContext httpContext)
    {
        var ownershipError = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipError is not null)
        {
            return ownershipError;
        }

        var result = await bus.InvokeAsync<Result<IEnumerable<TargetGroupDto>>>(new GetTargetGroupsQuery(scheduleId));
        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetTargetGroupById(
        Guid scheduleId,
        Guid groupId,
        IMessageBus bus,
        HttpContext httpContext)
    {
        var ownershipError = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipError is not null)
        {
            return ownershipError;
        }

        var result = await bus.InvokeAsync<Result<TargetGroupDto>>(new GetTargetGroupByIdQuery(groupId, scheduleId));
        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateTargetGroup(
        Guid scheduleId,
        CreateTargetGroupRequest request,
        IMessageBus bus,
        HttpContext httpContext)
    {
        var ownershipError = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipError is not null)
        {
            return ownershipError;
        }

        var command = new CreateTargetGroupCommand(
            scheduleId,
            request.Name,
            request.VillageCoordinates,
            request.Waves,
            request.BaseTemplateId,
            request.BaseTemplateName);

        var result = await bus.InvokeAsync<Result<TargetGroupDto>>(command);
        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Created($"/schedules/{scheduleId}/target-groups/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateTargetGroup(
        Guid scheduleId,
        Guid groupId,
        UpdateTargetGroupRequest request,
        IMessageBus bus,
        HttpContext httpContext)
    {
        var ownershipError = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipError is not null)
        {
            return ownershipError;
        }

        var command = new UpdateTargetGroupCommand(
            groupId,
            scheduleId,
            request.Name,
            request.VillageCoordinates,
            request.Waves,
            request.BaseTemplateId,
            request.BaseTemplateName);

        var result = await bus.InvokeAsync<Result<TargetGroupDto>>(command);
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

    private static async Task<IResult> DeleteTargetGroup(
        Guid scheduleId,
        Guid groupId,
        IMessageBus bus,
        HttpContext httpContext)
    {
        var ownershipError = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
        if (ownershipError is not null)
        {
            return ownershipError;
        }

        var command = new DeleteTargetGroupCommand(groupId, scheduleId);
        var result = await bus.InvokeAsync<Result>(command);
        if (result.IsFailure)
        {
            var errorMessage = result.Error ?? string.Empty;
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = result.Error });
            }

            return Results.BadRequest(new { error = result.Error });
        }

        return Results.NoContent();
    }
}
