using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Application.Users.DTOs;
using TWAction.IntegrationTests.Helpers;
using TWAction.Persistence;

namespace TWAction.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private const string CookieName = "TWAction.Session";

    public AuthEndpointsTests(TWActionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task GetMe_WithoutSessionCookie_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithInvalidSessionId_ReturnsUnauthorized()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Add("Cookie", $"{CookieName}=invalid-guid");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithNonExistentSessionId_ReturnsUnauthorized()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Add("Cookie", $"{CookieName}={Guid.NewGuid()}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidSession_ReturnsUserDto()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Add("Cookie", $"{CookieName}={session.Id}");

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();

        Assert.NotNull(userDto);
        Assert.Equal(user.Id, userDto.Id);
        Assert.Equal(user.Email, userDto.Email);
        Assert.Equal(user.DisplayName, userDto.DisplayName);
    }

    [Fact]
    public async Task GetMe_WithExpiredSession_ReturnsUnauthorized()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var session = await TestDataSeeder.SeedSessionAsync(
            dbContext,
            user.Id,
            expiresAt: DateTimeOffset.UtcNow.AddHours(-1));

        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Add("Cookie", $"{CookieName}={session.Id}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutSessionCookie_ReturnsNoContent()
    {
        var response = await _client.PostAsync("/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidSession_ReturnsNoContent()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        request.Headers.Add("Cookie", $"{CookieName}={session.Id}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidSession_DeletesSessionFromDatabase()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        request.Headers.Add("Cookie", $"{CookieName}={session.Id}");

        await _client.SendAsync(request);

        await using var verifyScope = _factory.Services.CreateAsyncScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var deletedSession = await verifyDbContext.UserSessions.FindAsync(session.Id);

        Assert.Null(deletedSession);
    }

    [Fact]
    public async Task Logout_WithValidSession_DeletesCookie()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        request.Headers.Add("Cookie", $"{CookieName}={session.Id}");

        var response = await _client.SendAsync(request);

        var setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        Assert.NotNull(setCookieHeader);
        Assert.Contains(CookieName, setCookieHeader);
    }

    [Fact]
    public async Task Logout_WithNonExistentSession_ReturnsNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        request.Headers.Add("Cookie", $"{CookieName}={Guid.NewGuid()}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
