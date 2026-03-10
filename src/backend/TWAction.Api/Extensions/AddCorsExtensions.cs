using TWAction.Api.Options;

namespace TWAction.Api.Extensions;

public static class AddCorsExtensions
{
    public const string AllowAllPolicy = "AllowAll";
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AllowAllPolicy, corsBuilder =>
            {
                var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>();

                if (corsOptions?.AllowedOrigins is null || corsOptions.AllowedOrigins.Length == 0)
                {
                    throw new InvalidOperationException(
                        "CORS AllowedOrigins must be configured in appsettings.json. " +
                        "Ensure the 'Cors:AllowedOrigins' section contains at least one origin.");
                }

                corsBuilder.WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
            });
        });
        return services;
    }
}
