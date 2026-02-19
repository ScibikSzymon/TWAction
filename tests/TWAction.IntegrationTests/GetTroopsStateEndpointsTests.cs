using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Application.Schedules.DTOs;
using TWAction.Domain.Users;
using TWAction.IntegrationTests.Helpers;
using TWAction.Persistence;

namespace TWAction.IntegrationTests;

public sealed class GetTroopsStateEndpointsTests : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public GetTroopsStateEndpointsTests(TWActionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task GetTroopsState_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        var scheduleId = Guid.NewGuid();

        var response = await _client.GetAsync($"/schedules/{scheduleId}/troops");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTroopsState_WhenScheduleIdIsEmpty_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/schedules/{Guid.Empty}/troops",
            session.Id);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Contains("Schedule ID must not be empty", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetTroopsState_WhenScheduleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var nonExistentScheduleId = Guid.NewGuid();

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/schedules/{nonExistentScheduleId}/troops",
            session.Id);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Contains("not found", body.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTroopsState_WhenScheduleBelongsToAnotherUser_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var otherUser = await TestDataSeeder.SeedUserAsync(dbContext, email: "other@example.com");
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: otherUser.Id);
        await TestDataSeeder.SeedTroopsStateAsync(dbContext, schedule.Id);

        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext, email: "requester@example.com");

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/schedules/{schedule.Id}/troops",
            session.Id);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Contains("Schedule not found for specified user", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetTroopsState_WhenAdminAccessesAnotherUsersSchedule_ReturnsOk()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var regularUser = await TestDataSeeder.SeedUserAsync(dbContext, email: "regular@example.com");
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: regularUser.Id);
        await TestDataSeeder.SeedTroopsStateAsync(dbContext, schedule.Id);

        var (_, adminSession) = await TestDataSeeder.SeedUserWithSessionAsync(
            dbContext, email: "admin@example.com", role: UserRole.Admin);

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/schedules/{schedule.Id}/troops",
            adminSession.Id);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TroopsStateDto>(_jsonOptions);
        Assert.NotNull(dto);
        Assert.Equal(schedule.Id, dto.ScheduleId);
    }

    [Fact]
    public async Task GetTroopsState_WhenTroopsStateDoesNotExist_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/schedules/{schedule.Id}/troops",
            session.Id);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Contains("Troops state for schedule", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetTroopsState_WhenValid_ReturnsOkWithCorrectDto()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user.Id);
        var troopsState = await TestDataSeeder.SeedTroopsStateAsync(dbContext, schedule.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/schedules/{schedule.Id}/troops",
            session.Id);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TroopsStateDto>(_jsonOptions);

        Assert.NotNull(dto);
        Assert.Equal(troopsState.Id, dto.Id);
        Assert.Equal(schedule.Id, dto.ScheduleId);
        Assert.Equal(3, dto.VillageCount);
        Assert.Equal(2, dto.PlayerCount);
    }

    [Fact]
    public async Task GetTroopsState_WhenValid_ReturnsDtoWithTimestamps()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user.Id);
        await TestDataSeeder.SeedTroopsStateAsync(dbContext, schedule.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/schedules/{schedule.Id}/troops",
            session.Id);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TroopsStateDto>(_jsonOptions);

        Assert.NotNull(dto);
        Assert.True(dto.CreatedAt > DateTimeOffset.MinValue);
        Assert.True(dto.UpdatedAt > DateTimeOffset.MinValue);
    }
}
