namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Application.DTOs;
using TWAction.Application.Handlers;
using Wolverine;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (IMessageBus bus) =>
        {
            var users = await bus.InvokeAsync<IEnumerable<UserDto>>(new GetAllUsersQuery());
            return Results.Json(users);
        });

        return app;
    }
}
