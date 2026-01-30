namespace ActionGenerator.Api.Endpoints;

using ActionGenerator.Application.Common;
using ActionGenerator.Application.Reconnaissance.Commands;
using ActionGenerator.Application.Reconnaissance.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public static class ActionEndpoints
{
    public static IEndpointRouteBuilder MapActionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/actions");

        group.MapPost("/reconnaissance", GenerateReconnaissanceActions)
            .WithName("GenerateReconnaissanceActions");

        return app;
    }

    private static async Task<IResult> GenerateReconnaissanceActions(
        GenerateReconnaissanceActionsRequest request,
        IValidator<GenerateReconnaissanceActionsCommand> validator,
        GenerateReconnaissanceActionsHandler handler)
    {
        var command = new GenerateReconnaissanceActionsCommand(
            request.MinDepartureTime,
            request.MinArrivalTime,
            request.MaxArrivalTime,
            request.MinDistanceToFront,
            request.MinSpyCount,
            request.MaxPopulationInSourceVillage,
            request.SkipNightSendings,
            request.SourceVillages,
            request.TargetVillages
        );

        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var result = await handler.Handle(command);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        return Results.Ok(result.Value);
    }
}

