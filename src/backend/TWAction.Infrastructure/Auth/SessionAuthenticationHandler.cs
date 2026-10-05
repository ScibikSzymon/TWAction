namespace TWAction.Infrastructure.Auth;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TWAction.Application.Common;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Queries;
using Wolverine;

public sealed class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "SessionAuth";

    private readonly IMessageBus _bus;
    private readonly AuthOptions _authOptions;

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IMessageBus bus,
        IOptions<AuthOptions> authOptions)
        : base(options, logger, encoder)
    {
        _bus = bus;
        _authOptions = authOptions.Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var cookieName = _authOptions.CookieName ?? "TWAction.Session";

        if (!Request.Cookies.TryGetValue(cookieName, out var sessionId) ||
            string.IsNullOrWhiteSpace(sessionId) ||
            !Guid.TryParse(sessionId, out var sessionGuid))
        {
            return AuthenticateResult.NoResult();
        }

        var userResult = await _bus.InvokeAsync<Result<UserDto>>(new GetUserBySessionQuery(sessionGuid));

        if (userResult is null || userResult.IsFailure)
        {
            return AuthenticateResult.Fail("Invalid or expired session");
        }

        var user = userResult.Value;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };

        if (!string.IsNullOrEmpty(user.DisplayName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}