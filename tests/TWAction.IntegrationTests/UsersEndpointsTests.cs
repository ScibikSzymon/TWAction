using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Application.Users.DTOs;
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
    public async Task GetUsers_WhenNoUsers_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/users");

        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        Assert.NotNull(users);
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetUsers_WhenUsersExist_ReturnsAllUsers()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        await TestDataSeeder.SeedMultipleUsersAsync(dbContext, 3);

        var response = await _client.GetAsync("/users");

        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        Assert.NotNull(users);
        Assert.Equal(3, users.Count);
    }

    [Fact]
    public async Task GetUsers_ReturnsCorrectUserDtoStructure()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var seededUser = await TestDataSeeder.SeedUserAsync(
            dbContext,
            email: "john.doe@example.com",
            displayName: "John Doe",
            provider: "google");

        var response = await _client.GetAsync("/users");

        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        Assert.NotNull(users);
        var user = Assert.Single(users);
        Assert.Equal(seededUser.Id, user.Id);
        Assert.Equal(seededUser.Email, user.Email);
        Assert.Equal(seededUser.DisplayName, user.DisplayName);
        Assert.Equal(seededUser.Provider, user.Provider);
        Assert.Equal(seededUser.CreatedAt.ToUnixTimeMilliseconds(), user.CreatedAt.ToUnixTimeMilliseconds());
    }

    [Fact]
    public async Task GetUsers_ReturnsOkStatusCode()
    {
        var response = await _client.GetAsync("/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/users");

        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}
