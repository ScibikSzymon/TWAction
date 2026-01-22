using Wolverine;
using Microsoft.Extensions.Options;
using TWAction.Api.Options;
using TWAction.Application.Handlers;
using TWAction.Application.Common;
using Microsoft.AspNetCore.Mvc;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Queries;
using TWAction.Application.Users.Commands;

namespace TWAction.Api.Endpoints;

public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication-related endpoints.
    /// </summary>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapGet("/me", async (HttpContext http, IOptions<AuthOptions> options, IMessageBus bus) =>
        {
            var cookieName = options.Value.CookieName ?? "TWAction.Session";

            // Return 401 so clients that gate on res.ok don't attempt res.json() for an empty (204) body.
            if (!http.Request.Cookies.TryGetValue(cookieName, out var sessionId) || string.IsNullOrWhiteSpace(sessionId))
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return Results.Unauthorized();
            }

            var user = await bus.InvokeAsync<Result<UserDto>>(new GetUserBySessionQuery(sessionGuid));
            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (user.IsFailure)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(user.Value);
        });

        group.MapPost("/logout", async (HttpContext http, IOptions<AuthOptions> options, IMessageBus bus) =>
        {
            var cookieName = options.Value.CookieName ?? "TWAction.Session";
            if (http.Request.Cookies.TryGetValue(cookieName, out var sessionId) && Guid.TryParse(sessionId, out var sessionGuid))
            {
                var result = await bus.InvokeAsync<Result>(new DeleteSessionCommand(sessionGuid));
                http.Response.Cookies.Delete(cookieName);

                if (!result.IsSuccess)
                {
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "Session Not Found",
                        Detail = result.Error,
                        Status = StatusCodes.Status404NotFound
                    });
                }
            }

            return Results.NoContent();
        });
    }
}
