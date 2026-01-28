using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TWAction.Infrastructure;
using TWAction.Persistence;
using TWAction.Api.Options;
using TWAction.Api.Endpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("Google"));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection("Cors"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON serialization to use string values for enums
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>();
        
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

var app = builder.Build();

app.UseCors("AllowAll");

// Apply EF Core migrations on startup in non-production environments
if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapLoginGoogleEndpoints();
app.MapUsersEndpoints();
app.MapAuthEndpoints();
app.MapScheduleEndpoints();
app.MapTroopsStateEndpoints();
app.MapTribesEndpoints();
app.MapReconnaissanceSettingsEndpoints();

app.Run();


// Expose a `Program` type for integration testing with `WebApplicationFactory<TEntryPoint>`.
// This keeps the top-level statements while providing a concrete type the test
// project can reference as the generic parameter to `WebApplicationFactory<T>`.
public partial class Program { }
