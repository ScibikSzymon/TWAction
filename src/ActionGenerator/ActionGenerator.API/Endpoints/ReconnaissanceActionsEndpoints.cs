using ActionGenerator.API.Filters;
using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;
using ActionGenerator.Application.Features.ReconnaissanceActions.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ActionGenerator.API.Endpoints;

public static class ReconnaissanceActionsEndpoints
{
    public static void MapReconnaissanceActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reconnaissance-actions");

        group.MapPost("/generate", GenerateReconnaissanceActionsAsync)
            .WithName("GenerateReconnaissanceActions");

        group.AddEndpointFilter<ValidationFilter<GenerateReconnaissanceActionsRequest>>();
    }

    private static async Task<Results<Ok<IReadOnlyList<AttackCommandDto>>, ValidationProblem>> GenerateReconnaissanceActionsAsync(
        GenerateReconnaissanceActionsRequest request,
        IReconnaissanceActionsService service,
        CancellationToken cancellationToken)
    {
        var commands = await service.GenerateAsync(request, cancellationToken);
        
        return TypedResults.Ok(commands);
    }
}


