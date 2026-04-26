namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TWAction.Api.Extensions;
using TWAction.Application.Common;
using TWAction.Application.Users.Commands;
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
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        app.MapDelete("/users/{userId:guid}", async (Guid userId, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result>(new DeleteUserCommand(userId));

            if (result.IsFailure)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Results.NoContent();
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return app;
    }
}
