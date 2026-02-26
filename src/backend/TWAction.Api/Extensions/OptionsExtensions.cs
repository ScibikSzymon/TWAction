using TWAction.Api.Options;
using TWAction.Infrastructure.Auth;

namespace TWAction.Api.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddApiOptions(this IServiceCollection services)
    {
        services.AddOptions<GoogleOptions>()
            .BindConfiguration(GoogleOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .BindConfiguration(CorsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
