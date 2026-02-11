namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.Settings.Commands;
using TWAction.Application.Settings.DTOs;
using TWAction.Application.Settings.Queries;
using Wolverine;

public static class ReconnaissanceSettingsEndpoints
{
    public static IEndpointRouteBuilder MapReconnaissanceSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/reconnaissance")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);;

        group.MapGet("", GetReconnaissanceSettings)
            .WithName("GetReconnaissanceSettings");

        group.MapPut("", SaveReconnaissanceSettings)
            .WithName("SaveReconnaissanceSettings");

        return app;
    }

    private static async Task<IResult> GetReconnaissanceSettings(
        Guid scheduleId,
        IMessageBus bus)
    {
        var query = new GetReconnaissanceSettingsQuery(scheduleId);

        var result = await bus.InvokeAsync<Result<ReconnaissanceSettingsDto>>(query);

        if (result.IsFailure)
        {
            return Results.NotFound(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SaveReconnaissanceSettings(
        Guid scheduleId,
        SaveReconnaissanceSettingsRequest request,
        IMessageBus bus)
    {
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            request.MinDepartureTime,
            request.MinArrivalTime,
            request.MaxArrivalTime,
            request.MinDistanceToFront,
            request.MinSpyCount,
            request.MaxPopulationInSourceVillage,
            request.SkipNightSendings
        );

        var result = await bus.InvokeAsync<Result<ReconnaissanceSettingsDto>>(command);

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
