using Microsoft.EntityFrameworkCore;
using TWAction.Infrastructure;
using TWAction.Persistence;
using TWAction.Api.Endpoints;
using TWAction.Api.Validators;
using TWAction.Api.Extensions;
using FluentValidation;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddApiOptions(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssemblyContaining<CreateScheduleRequestValidator>();

// Configure JSON serialization to use string values for enums
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.LogConfigurationValues(builder.Configuration);
}

app.UseCors(AddCorsExtensions.AllowAllPolicy);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapLoginGoogleEndpoints();
app.MapUsersEndpoints();
app.MapAuthEndpoints();
app.MapScheduleEndpoints();
app.MapTroopsStateEndpoints();
app.MapTribesEndpoints();

app.Run();

// Expose a `Program` type for integration testing with `WebApplicationFactory<TEntryPoint>`.
// This keeps the top-level statements while providing a concrete type the test
// project can reference as the generic parameter to `WebApplicationFactory<T>`.
public partial class Program { }
