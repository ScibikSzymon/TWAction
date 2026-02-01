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
        var group = app.MapGroup("/api/reconnaissance-actions");

        group.MapPost("/generate", GenerateReconnaissanceActionsAsync)
            .WithName("GenerateReconnaissanceActions");
    }

    private static async Task<Results<Ok<IReadOnlyList<AttackCommandDto>>, ValidationProblem>> GenerateReconnaissanceActionsAsync(
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

        var commands = await service.GenerateAsync(request, cancellationToken);
        
        return TypedResults.Ok(commands);
    }
}


