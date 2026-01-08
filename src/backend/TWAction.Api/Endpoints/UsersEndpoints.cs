namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (IServiceProvider services) =>
        {
            using var scope = services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TWAction.Application.Handlers.GetAllUsersHandler>();
            var users = await handler.Handle(new TWAction.Application.Queries.GetAllUsersQuery());
            return Results.Json(users);
        });

        return app;
    }
}
