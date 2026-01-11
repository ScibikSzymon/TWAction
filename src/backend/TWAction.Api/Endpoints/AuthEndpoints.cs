using Wolverine;
using Microsoft.Extensions.Options;
using TWAction.Api.Options;
using TWAction.Application.DTOs;
using TWAction.Application.Handlers;

namespace TWAction.Api.Endpoints
{
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

                var user = await bus.InvokeAsync<UserDto?>(new GetUserBySessionQuery(sessionGuid));
                if (user is null)
                {
                    return Results.Unauthorized();
                }

                // Map to frontend-compatible response shape
                // var response = new
                // {
                //     id = user.Id.ToString(),
                //     email = user.Email,
                //     displayName = user.DisplayName,
                //     provider = user.Provider,
                //     createdAt = user.CreatedAt.ToString("O") // ISO 8601 format
                // };

                return Results.Ok(user);
            });

            group.MapPost("/logout", async (HttpContext http, IOptions<AuthOptions> options, IMessageBus bus) =>
            {
                var cookieName = options.Value.CookieName ?? "TWAction.Session";
                if (http.Request.Cookies.TryGetValue(cookieName, out var sessionId) && Guid.TryParse(sessionId, out var sessionGuid))
                {
                    await bus.InvokeAsync(new DeleteSessionCommand(sessionGuid));
                    http.Response.Cookies.Delete(cookieName);
                }

                return Results.NoContent();
            });
        }
    }
}
