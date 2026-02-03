using ActionGenerator.Api.Endpoints;
using ActionGenerator.Api.Security;
using Scalar.AspNetCore;
using ActionGenerator.Application;
using ActionGenerator.Infrastructure;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();
        builder.Services.AddAuthentication(ApiKeyAuthenticationDefaults.Scheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationDefaults.Scheme,
                options => options.ApiKey = builder.Configuration["Authentication:ApiKey"] ?? string.Empty);
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
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

        app.UseHttpsRedirection();

        app.MapReconnaissanceActionsEndpoints();

        app.Run();
    }
}