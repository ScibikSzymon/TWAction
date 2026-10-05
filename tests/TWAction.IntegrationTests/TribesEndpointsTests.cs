using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Users;
using TWAction.IntegrationTests.Helpers;
using TWAction.Persistence;

namespace TWAction.IntegrationTests;

public sealed class TribesEndpointsTests : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TribesEndpointsTests(TWActionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = CreateClientWithStubbedTribesService(factory);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task GetTribes_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/worlds/pl218/tribes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTribes_WithUserRole_ReturnsOk()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/worlds/pl218/tribes", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTribes_WithAdminRole_ReturnsOk()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        user.Role = UserRole.Admin;
        await dbContext.SaveChangesAsync();
        var session = await TestDataSeeder.SeedSessionAsync(dbContext, user.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/worlds/pl218/tribes", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static HttpClient CreateClientWithStubbedTribesService(TWActionWebApplicationFactory factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITribesService>();
                services.AddSingleton<ITribesService, StubTribesService>();
            });
        }).CreateClient();
    }
}
