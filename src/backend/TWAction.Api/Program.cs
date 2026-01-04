using Microsoft.EntityFrameworkCore;
using TWAction.Infrastructure;
using TWAction.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register infrastructure and persistence (Wolverine, DbContext, repositories)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("OpenPolicy");

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

app.MapGet("/", () => "Hello World!");

app.Run();

// Expose a `Program` type for integration testing with `WebApplicationFactory<TEntryPoint>`.
// This keeps the top-level statements while providing a concrete type the test
// project can reference as the generic parameter to `WebApplicationFactory<T>`.
public partial class Program { }
