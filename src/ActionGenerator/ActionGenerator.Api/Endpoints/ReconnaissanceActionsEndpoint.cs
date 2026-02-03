using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;
using ActionGenerator.Application.Features.ReconnaissanceActions.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ActionGenerator.Api;

/// <summary>
/// Provides API endpoints for reconnaissance action generation.
/// </summary>
public static class ReconnaissanceActionsEndpoint
{
    /// <summary>
    /// Maps reconnaissance action endpoints onto the route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapReconnaissanceActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Api/reconnaissance-actions");
        group.RequireAuthorization();

        group.MapPost("", GenerateReconnaissanceActions)
            .WithName("GenerateReconnaissanceActions");

        group.AddEndpointFilter<ValidationFilter<GenerateReconnaissanceActionsRequest>>();
    }

    // Generates reconnaissance actions using the application service.
    private static Results<Ok<IReadOnlyList<AttackCommandDto>>, ValidationProblem> GenerateReconnaissanceActions(
        GenerateReconnaissanceActionsRequest request,
        IReconnaissanceActionsService service,
        CancellationToken cancellationToken)
    {
        var commands = service.Generate(request, cancellationToken);
        
        return TypedResults.Ok(commands);
    }
}


