using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Application.Users.DTOs;
using TWAction.Domain.Users;
using TWAction.IntegrationTests.Helpers;
using TWAction.Persistence;

namespace TWAction.IntegrationTests;

public sealed class UsersEndpointsTests : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointsTests(TWActionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task GetUsers_WhenAuthenticatedUserExists_ReturnsSingleUser()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext, role: UserRole.Admin);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/users", session.Id);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        Assert.NotNull(users);
        Assert.Single(users); // The authenticated user
    }

    [Fact]
    public async Task GetUsers_WhenUsersExist_ReturnsAllUsers()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext, role: UserRole.Admin);
        await TestDataSeeder.SeedMultipleUsersAsync(dbContext, 3);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/users", session.Id);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        Assert.NotNull(users);
        Assert.Equal(4, users.Count); // 3 + authenticated user
    }

    [Fact]
    public async Task GetUsers_ReturnsCorrectUserDtoStructure()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext, role: UserRole.Admin);
        var seededUser = await TestDataSeeder.SeedUserAsync(
            dbContext,
            email: "john.doe@example.com",
            displayName: "John Doe",
            provider: "google");

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/users", session.Id);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
        var user = users.FirstOrDefault(u => u.Email == "john.doe@example.com");
        Assert.NotNull(user);
        Assert.Equal(seededUser.Id, user.Id);
        Assert.Equal(seededUser.Email, user.Email);
        Assert.Equal(seededUser.DisplayName, user.DisplayName);
        Assert.Equal(seededUser.Provider, user.Provider);
        Assert.Equal(seededUser.CreatedAt.ToUnixTimeMilliseconds(), user.CreatedAt.ToUnixTimeMilliseconds());
    }

    [Fact]
    public async Task GetUsers_ReturnsOkStatusCode()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext, role: UserRole.Admin);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/users", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ReturnsJsonContentType()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext, role: UserRole.Admin);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/users", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetUsers_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, "/users", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
