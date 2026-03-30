using ActionGenerator.Api.Filters;
using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Features.MainActions.Dtos;
using ActionGenerator.Application.Features.MainActions.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ActionGenerator.Api.Endpoints;

/// <summary>
/// Provides API endpoints for main action generation (Off, Catas, FakeOff, FakeDeff, Nobles).
/// </summary>
public static class MainActionsEndpoint
{
    /// <summary>
    /// Maps main action endpoints onto the route builder.
    /// </summary>
    public static void MapMainActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Api/main-actions");
        //group.RequireAuthorization();

        group.MapPost("", GenerateMainActions)
            .WithName("GenerateMainActions");

        group.AddEndpointFilter<FilterForValidation<GenerateMainActionRequest>>();
    }

    // Generates main action commands using the application service.
    private static Results<Ok<IReadOnlyList<AttackCommandDto>>, ValidationProblem> GenerateMainActions(
        GenerateMainActionRequest request,
        IMainActionsService service,
        CancellationToken cancellationToken)
    {
        var commands = service.Generate(request, cancellationToken);

        return TypedResults.Ok(commands);
    }
}
