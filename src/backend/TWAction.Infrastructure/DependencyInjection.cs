using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Wolverine;
using TWAction.Application.Interfaces;
using TWAction.Persistence;
using TWAction.Persistence.Repositories;
using TWAction.Infrastructure.Services;
using TWAction.Infrastructure.Auth;
using TWAction.Application.Handlers;
using TWAction.Application.Users.Queries;
using TWAction.Application.Users.Interfaces;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Queries;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Users.Commands;
using TWAction.Application.Schedules.Services;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Application.Tribes.Queries;
using TWAction.Application.Settings.Interfaces;
using TWAction.Application.Settings.Queries;
using TWAction.Application.Settings.Commands;


namespace TWAction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("TWActionDatabase");
        services.AddDbContext<TWActionDbContext>(opts => opts.UseNpgsql(conn));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        // Right now this keeps the default in-process bus (no external transports) 
        services.AddWolverine(opts =>
        {
            opts.Durability.Mode = DurabilityMode.MediatorOnly;
            opts.Discovery.IncludeAssembly(typeof(SignInWithGoogleHandler).Assembly);
        });

        // Register HttpClient factory and IMemoryCache for TribalWars Api calls
        services.AddHttpClient<TribesHttpService>();
        services.AddMemoryCache();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ITroopsStateRepository, TroopsStateRepository>();
        services.AddScoped<IReconnaissanceSettingsRepository, ReconnaissanceSettingsRepository>();


        services.AddSingleton<TroopsStateValidator>();
        services.AddSingleton<TroopsStateCompressionService>();
        services.AddSingleton<TroopsStateStatsExtractor>();
        services.AddSingleton<TribesCsvParser>();
        services.AddScoped<ITribesService, TribesHttpService>();

        services.AddTransient<SignInWithGoogleHandler>();
        services.AddTransient<GetAllUsersHandler>();
        services.AddTransient<GetUserBySessionHandler>();
        services.AddTransient<DeleteSessionHandler>();
        services.AddTransient<GetAllSchedulesHandler>();
        services.AddTransient<GetScheduleByIdHandler>();
        services.AddTransient<CreateScheduleHandler>();
        services.AddTransient<UpdateScheduleHandler>();
        services.AddTransient<DeleteScheduleHandler>();
        services.AddTransient<UploadTroopsStateHandler>();
        services.AddTransient<GetTroopsStateHandler>();
        services.AddTransient<GetTribesHandler>();
        services.AddTransient<GetReconnaissanceSettingsHandler>();
        services.AddTransient<SaveReconnaissanceSettingsHandler>();

        // Register authentication with session-based authentication handler
        services.AddAuthentication(SessionAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
                SessionAuthenticationHandler.SchemeName, 
                options => { });

        return services;
    }
}

