using ActionGenerator.Api.Options;
using Microsoft.Extensions.Options;

namespace ActionGenerator.Api.Middleware;

/// <summary>
/// Middleware for API Key authentication
/// Validates X-API-Key header against configured valid keys
/// </summary>
public sealed class ApiKeyAuthMiddleware(
    RequestDelegate next,
    IOptions<ApiKeyOptions> options,
    ILogger<ApiKeyAuthMiddleware> logger)
{
    private readonly ApiKeyOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth for health check and swagger endpoints
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        // Extract API key from header
        if (!context.Request.Headers.TryGetValue(_options.HeaderName, out var extractedApiKey))
        {
            logger.LogWarning("API Key missing in request from {RemoteIp}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API Key missing" });
            return;
        }

        // Validate API key
        if (!_options.ValidKeys.Contains(extractedApiKey.ToString()))
        {
            logger.LogWarning("Invalid API Key attempt from {RemoteIp}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
            return;
        }

        await next(context);
    }
}
