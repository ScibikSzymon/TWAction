namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.Settings.Commands;
using TWAction.Application.Settings.DTOs;
using TWAction.Application.Settings.Queries;
using Wolverine;

public static class MainActionSettingsEndpoints
{
    public static IEndpointRouteBuilder MapMainActionSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/mainaction")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapGet("", GetMainActionSettings)
            .WithName("GetMainActionSettings");

        group.MapPut("", SaveMainActionSettings)
            .WithName("SaveMainActionSettings");

        return app;
    }

    private static async Task<IResult> GetMainActionSettings(
        Guid scheduleId,
        IMessageBus bus)
    {
        var query = new GetMainActionSettingsQuery(scheduleId);

        var result = await bus.InvokeAsync<Result<MainActionSettingsDto>>(query);

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SaveMainActionSettings(
        Guid scheduleId,
        SaveMainActionSettingsRequest request,
        IMessageBus bus)
    {
        var command = new SaveMainActionSettingsCommand(
            scheduleId,
            request.MinDepartureTime,
            request.SkipNightSendings,
            request.MaxNobleDistance,
            request.ActionDate,
            request.OffSettings,
            request.CatasSettings,
            request.FakeOffSettings,
            request.FakeDeffSettings,
            request.NobleSettings,
            request.PlayerNobleBudgets
        );

        var result = await bus.InvokeAsync<Result<MainActionSettingsDto>>(command);

        if (result.IsFailure)
        {
            var errorMessage = result.Error ?? string.Empty;

            // Return 404 for not-found schedule errors to be consistent with other endpoints.
            if (errorMessage.Contains("not found", System.StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = result.Error });
            }
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}
