using ActionGenerator.Api.Endpoints;
using ActionGenerator.Api.Middleware;
using ActionGenerator.Api.Options;
using ActionGenerator.Application;
using ActionGenerator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services by layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection("Cors"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
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

// API Key Authentication Middleware
app.UseMiddleware<ApiKeyAuthMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapActionEndpoints();

app.MapDefaultEndpoints();

app.Run();

// Expose Program type for integration testing
public partial class Program { }

