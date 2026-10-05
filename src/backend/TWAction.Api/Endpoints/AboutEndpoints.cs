using TWAction.Application.Users.Interfaces;

namespace TWAction.Api.Endpoints;

public static class AboutEndpoints
{
    public static IEndpointRouteBuilder MapAboutEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/about/limits", GetDefaultLimits)
            .WithName("GetDefaultSubscriptionLimits");

        return app;
    }

    private static async Task<IResult> GetDefaultLimits(
        ISubscriptionPlanLimitsRepository repository,
        CancellationToken cancellationToken)
    {
        var plans = await repository.ListAsync(cancellationToken);
        var response = plans.Select(plan => new SubscriptionPlanLimitsResponse(
            plan.SubscriptionTier.ToString(),
            plan.ScheduleLimit,
            plan.TemplateLimit,
            plan.TroopsUploadLimit,
            plan.TroopsUploadWindowHours));

        return Results.Ok(response);
    }
}

public sealed record SubscriptionPlanLimitsResponse(
    string SubscriptionTier,
    int? ScheduleLimit,
    int? TemplateLimit,
    int? TroopsUploadLimit,
    int TroopsUploadWindowHours);
