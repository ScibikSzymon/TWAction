using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Api.Extensions;
using TWAction.Application.ReconnaissanceActions.Commands;
using TWAction.Application.ReconnaissanceActions.Handlers;
using Wolverine;

namespace TWAction.Api.Endpoints;

public static class ReconnaissanceActionsEndpoints
{
    public static IEndpointRouteBuilder MapReconnaissanceActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schedules/{scheduleId}/reconnaissance/actions")
            .RequireAuthorization(AuthorizationPolicies.UserOrAbove);

        group.MapPost("", GenerateReconnaissanceActions)
            .WithName("GenerateReconnaissanceActions");

        return app;
    }

    private static async Task<IResult> GenerateReconnaissanceActions(
        Guid scheduleId,
        HttpContext httpContext,
        GenerateReconnaissanceActionsHandler handler,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        // Validate schedule ownership
        var ownershipResult = await ScheduleOwnershipHelper.ValidateOwnershipAsync(scheduleId, httpContext, bus);
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

}
