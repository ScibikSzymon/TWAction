using ActionGenerator.Application.Common.DTOs;
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
            .WithDescription(
                "Generates optimal reconnaissance (spy) attack commands based on ally and enemy village data. " +
                "The algorithm calculates travel times, respects time windows, and can skip night-time sendings (22:00-08:00).")
            .Produces<IReadOnlyList<AttackCommandDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }

    private static async Task<Results<Ok<IReadOnlyList<AttackCommandDto>>, ValidationProblem>> GenerateReconnaissanceActionsAsync(
        GenerateReconnaissanceActionsRequest request,
        IReconnaissanceActionsService service,
        IValidator<GenerateReconnaissanceActionsRequest> validator,
        CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Generate reconnaissance commands
        var commands = await service.GenerateAsync(request, cancellationToken);
        
        return TypedResults.Ok(commands);
    }
}


