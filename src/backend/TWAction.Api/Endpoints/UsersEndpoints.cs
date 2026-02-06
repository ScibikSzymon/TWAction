namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TWAction.Application.Common;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Queries;
using Wolverine;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (IMessageBus bus) =>
        {
            var users = await bus.InvokeAsync<Result<IEnumerable<UserDto>>>(new GetAllUsersQuery());
            return Results.Json(users.Value);
        }).RequireAuthorization();

        return app;
    }
}
