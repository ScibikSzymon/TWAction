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

    /// <summary>
    /// Initializes a new instance of the <see cref="TribesEndpointsTests"/> class.
    /// </summary>
    /// <param name="factory">The shared test web application factory.</param>
    public TribesEndpointsTests(TWActionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = CreateClientWithStubbedTribesService(factory);
    }

    /// <summary>
    /// Performs per-test initialization.
    /// </summary>
    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Cleans up database state after each test.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    /// <summary>
    /// Ensures anonymous access is rejected.
    /// </summary>
    [Fact]
    public async Task GetTribes_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/worlds/pl218/tribes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Ensures standard users can access user-or-above tribes endpoints.
    /// </summary>
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

    /// <summary>
    /// Ensures admins can access tribes endpoints.
    /// </summary>
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

    /// <summary>
    /// Creates a client that uses a deterministic tribes service implementation.
    /// </summary>
    /// <param name="factory">The base factory to configure.</param>
    /// <returns>An <see cref="HttpClient"/> with the stubbed service.</returns>
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
