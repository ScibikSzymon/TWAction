namespace TWAction.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using TWAction.Api.Options;
using TWAction.Application.Handlers;

public static class LoginGoogleEndpoints
{
    public static IEndpointRouteBuilder MapLoginGoogleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/google", (HttpContext http, IOptions<GoogleOptions> googleOptions) =>
        {
            var opts = googleOptions.Value;
            var clientId = opts?.ClientId ?? "";
            var redirectUri = opts?.RedirectUri ?? "https://localhost:5001/auth/google/callback";
            var scope = "openid email profile";
            var state = Guid.NewGuid().ToString("N");

            var url = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString(scope)}&state={state}&access_type=offline&prompt=consent";
            return Results.Redirect(url);
        });

        app.MapGet("/auth/google/callback", async (HttpRequest request, HttpResponse response, IServiceProvider services, IOptions<GoogleOptions> googleOptions, IOptions<AuthOptions> authOptions) =>
        {
            var q = request.Query;
            if (!q.TryGetValue("code", out var codeVals)) return Results.BadRequest(new { error = "Missing code" });
            var code = codeVals.ToString();

            var opts = googleOptions.Value;
            var clientId = opts?.ClientId ?? string.Empty;
            var clientSecret = opts?.ClientSecret ?? string.Empty;
            var redirectUri = opts?.RedirectUri ?? "https://localhost:5001/auth/google/callback";

            var idToken = await ExchangeCodeForIdTokenAsync(code, clientId, clientSecret, redirectUri);
            if (string.IsNullOrEmpty(idToken)) return Results.StatusCode(502);

            using var payloadJson = DecodeIdTokenPayload(idToken);
            if (payloadJson is null) return Results.StatusCode(502);

            var email = payloadJson.RootElement.GetProperty("email").GetString() ?? string.Empty;
            var name = payloadJson.RootElement.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;

            var result = await SignInAndCreateSessionAsync(services, email, name);
            if (result is null) return Results.StatusCode(502);

            SetSessionCookie(response, authOptions, result.SessionId);
            return Results.Json(new { success = true });
        });

        return app;
    }

    private static async Task<string?> ExchangeCodeForIdTokenAsync(string code, string clientId, string clientSecret, string redirectUri)
    {
        using var http = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            })
        };

        var tokenResp = await http.SendAsync(tokenRequest);
        if (!tokenResp.IsSuccessStatusCode) return null;

        var tokenJson = await tokenResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(tokenJson);
        if (!doc.RootElement.TryGetProperty("id_token", out var idTokenEl)) return null;
        return idTokenEl.GetString();
    }

    private static JsonDocument? DecodeIdTokenPayload(string idToken)
    {
        string[] parts = idToken.Split('.');
        if (parts.Length < 2) return null;
        string payload = parts[1];
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
        return JsonDocument.Parse(bytes);
    }

    private static async Task<Application.DTOs.SignInResult?> SignInAndCreateSessionAsync(IServiceProvider services, string email, string? displayName)
    {
        using var scope = services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<SignInWithGoogleHandler>();
        var result = await handler.Handle(new SignInWithGoogleCommand(email, displayName, "google"));
        return result;
    }

    private static void SetSessionCookie(HttpResponse response, IOptions<AuthOptions> authOptions, Guid sessionId)
    {
        var cookieName = authOptions?.Value?.CookieName ?? "TWAction.Session";
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8).UtcDateTime
        };

        response.Cookies.Append(cookieName, sessionId.ToString(), cookieOptions);
    }
}
