namespace TWAction.Api.Extensions;

public static class ConfigurationLoggingExtensions
{
    public static void LogConfigurationValues(this WebApplication app, IConfiguration configuration)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("=== Configuration Values ===");
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

        foreach (var config in configuration.AsEnumerable().OrderBy(c => c.Key))
        {
            var value = config.Key.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
                        config.Key.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                        config.Key.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase)
                ? "***MASKED***"
                : config.Value;

            logger.LogInformation("{Key} = {Value}", config.Key, value);
        }

        logger.LogInformation("============================");
    }
}
