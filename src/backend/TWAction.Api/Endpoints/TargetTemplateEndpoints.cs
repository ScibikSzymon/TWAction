namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Templates.Commands;
using TWAction.Application.Templates.DTOs;
using TWAction.Application.Templates.Queries;
using Wolverine;

public static class TargetTemplateEndpoints
{
    public static IEndpointRouteBuilder MapTargetTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/target-templates")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("", GetTemplates)
            .WithName("GetTargetTemplates");

        group.MapGet("/{templateId}", GetTemplateById)
            .WithName("GetTargetTemplateById");

        group.MapPost("", CreateTemplate)
            .WithName("CreateTargetTemplate");

        group.MapPut("/{templateId}", UpdateTemplate)
            .WithName("UpdateTargetTemplate");

        group.MapDelete("/{templateId}", DeleteTemplate)
            .WithName("DeleteTargetTemplate");

        return app;
    }

    private static async Task<IResult> GetTemplates(
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await bus.InvokeAsync<Result<IEnumerable<TargetTemplateDto>>>(
            new GetTargetTemplatesQuery(userId));

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetTemplateById(
        Guid templateId,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await bus.InvokeAsync<Result<TargetTemplateDto>>(
            new GetTargetTemplateByIdQuery(templateId, userId));

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateTemplate(
        CreateTargetTemplateRequest request,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new CreateTargetTemplateCommand(userId, request.Name, request.Waves);
        var result = await bus.InvokeAsync<Result<TargetTemplateDto>>(command);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Created($"/target-templates/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateTemplate(
        Guid templateId,
        UpdateTargetTemplateRequest request,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new UpdateTargetTemplateCommand(templateId, userId, request.Name, request.Waves);
        var result = await bus.InvokeAsync<Result<TargetTemplateDto>>(command);

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

    private static async Task<IResult> DeleteTemplate(
        Guid templateId,
        IMessageBus bus,
        ICurrentUserAccessor currentUser)
    {
        if (!currentUser.TryGetUserId(out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new DeleteTargetTemplateCommand(templateId, userId);
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
