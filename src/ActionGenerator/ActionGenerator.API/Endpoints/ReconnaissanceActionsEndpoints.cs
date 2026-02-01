using ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;
using ActionGenerator.Application.Features.ReconnaissanceActions.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ActionGenerator.API.Endpoints;

public static class ReconnaissanceActionsEndpoints
{
    public static void MapReconnaissanceActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reconnaissance-actions")
            .WithTags("Reconnaissance Actions")
            .WithDescription("Generate reconnaissance attack commands for Tribal Wars")
            .WithOpenApi();

        group.MapPost("/generate", GenerateReconnaissanceActionsAsync)
            .WithName("GenerateReconnaissanceActions")
            .WithSummary("Generate reconnaissance attack commands")
            .WithDescription("Generates optimal reconnaissance (spy) attack commands based on ally and enemy village data. " +
                           "The algorithm calculates travel times, respects time windows, and can skip night-time sendings (22:00-08:00).")
            .Produces<GenerateReconnaissanceActionsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }

    private static async Task<Results<Ok<GenerateReconnaissanceActionsResponse>, ValidationProblem>> GenerateReconnaissanceActionsAsync(
        GenerateReconnaissanceActionsRequest request,
        IReconnaissanceActionsService service,
        IValidator<GenerateReconnaissanceActionsRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await service.GenerateAsync(request, cancellationToken);
        return TypedResults.Ok(response);
    }
}

