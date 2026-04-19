using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using ActionGenerator.Application;
using ActionGenerator.Infrastructure;
using ActionGenerator.Api.Endpoints;
using ActionGenerator.Api.Security;

namespace ActionGenerator.Api;

public partial class Program
{
    // Configures and starts the ActionGenerator API host.
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var apiKey = builder.Configuration["Authentication:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "API key is not configured. Please set 'Authentication:ApiKey' in appsettings.json or environment variables.");
        }

        builder.Services.AddProblemDetails();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddAuthentication(ApiKeyAuthenticationDefaults.Scheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationDefaults.Scheme,
                options => options.ApiKey = apiKey);
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapReconnaissanceActionsEndpoints();
        app.MapMainActionsEndpoints();

        app.MapHealthChecks("/health");

        app.Run();
    }
}