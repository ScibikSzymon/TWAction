using ActionGenerator.Api.Filters;
using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Features.MainActions.Dtos;
using ActionGenerator.Application.Features.MainActions.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ActionGenerator.Api.Endpoints;

public static class MainActionsEndpoint
{
    public static void MapMainActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Api/main-actions");
        //group.RequireAuthorization();

        group.MapPost("", GenerateMainActions)
            .WithName("GenerateMainActions");

        group.AddEndpointFilter<FilterForValidation<GenerateMainActionRequest>>();
    }

    private static Results<Ok<IReadOnlyList<AttackCommandDto>>, ValidationProblem> GenerateMainActions(
        GenerateMainActionRequest request,
        IMainActionsService service,
        CancellationToken cancellationToken)
    {
        var commands = service.Generate(request, cancellationToken);

        return TypedResults.Ok(commands);
    }
}
