namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TWAction.Api.Extensions;
using TWAction.Api.Filters;
using TWAction.Application.Common;
using TWAction.Application.Users.Commands;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Queries;
using TWAction.Domain.Users;
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

        app.MapGet("/users/{userId:guid}", async (Guid userId, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result<UserDto>>(new GetUserByIdQuery(userId));

            return result.IsFailure
                ? Results.NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound
                })
                : Results.Ok(result.Value);
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        app.MapPut("/users/{userId:guid}", async (
            Guid userId,
            UpdateUserRequest request,
            IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result<UserDto>>(new UpdateUserCommand(userId, request));

            if (result.IsFailure)
            {
                var isDuplicateEmail = result.Error.Contains("already uses this email", StringComparison.OrdinalIgnoreCase);
                return isDuplicateEmail
                    ? Results.Conflict(new ProblemDetails
                    {
                        Title = "Email already in use",
                        Detail = result.Error,
                        Status = StatusCodes.Status409Conflict
                    })
                    : Results.NotFound(new ProblemDetails
                    {
                        Title = "User Not Found",
                        Detail = result.Error,
                        Status = StatusCodes.Status404NotFound
                    });
            }

            return Results.Ok(result.Value);
        })
        .AddEndpointFilter<ValidationFilter<UpdateUserRequest>>()
        .RequireAuthorization(AuthorizationPolicies.AdminOnly);

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

        app.MapGet("/users/{userId:guid}/sessions", async (Guid userId, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result<IEnumerable<UserSessionDto>>>(new GetUserSessionsQuery(userId));

            return result.IsFailure
                ? Results.NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound
                })
                : Results.Ok(result.Value);
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        app.MapDelete("/users/{userId:guid}/sessions/{sessionId:guid}", async (
            Guid userId,
            Guid sessionId,
            IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result>(new DeleteUserSessionCommand(userId, sessionId));

            return result.IsFailure
                ? Results.NotFound(new ProblemDetails
                {
                    Title = "Session Not Found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound
                })
                : Results.NoContent();
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        app.MapDelete("/users/{userId:guid}/sessions", async (Guid userId, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result>(new DeleteUserSessionsCommand(userId));

            return result.IsFailure
                ? Results.NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound
                })
                : Results.NoContent();
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return app;
    }
}
