using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Api.Endpoints;
using TWAction.Application.Schedules.DTOs;
using TWAction.Domain.Schedules;
using TWAction.IntegrationTests.Helpers;
using TWAction.Persistence;

namespace TWAction.IntegrationTests;

public sealed class ScheduleEndpointsTests : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ScheduleEndpointsTests(TWActionWebApplicationFactory factory)
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
    public async Task GetSchedulesByUser_WhenNoSchedules_ReturnsEmptyList()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user.Id}", session.Id);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var schedules = await response.Content.ReadFromJsonAsync<List<ScheduleDto>>(_jsonOptions);

        Assert.NotNull(schedules);
        Assert.Empty(schedules);
    }

    [Fact]
    public async Task GetSchedulesByUser_WhenSchedulesExist_ReturnsAllSchedulesForUser()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        await TestDataSeeder.SeedMultipleSchedulesAsync(dbContext, user.Id, 3);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user.Id}", session.Id);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var schedules = await response.Content.ReadFromJsonAsync<List<ScheduleDto>>(_jsonOptions);

        Assert.NotNull(schedules);
        Assert.Equal(3, schedules.Count);
    }

    [Fact]
    public async Task GetSchedulesByUser_ReturnsCorrectScheduleDtoStructure()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var seededSchedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            name: "Test Schedule",
            world: WorldType.pl219,
            scheduleType: ScheduleType.Reconnaissance);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user.Id}", session.Id);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var schedules = await response.Content.ReadFromJsonAsync<List<ScheduleDto>>(_jsonOptions);

        Assert.NotNull(schedules);
        var schedule = Assert.Single(schedules);
        Assert.Equal(seededSchedule.Id, schedule.Id);
        Assert.Equal(seededSchedule.UserGuid, schedule.UserId);
        Assert.Equal(seededSchedule.Name, schedule.Name);
        Assert.Equal(seededSchedule.World, schedule.World);
        Assert.Equal(seededSchedule.ScheduleType, schedule.ScheduleType);
    }

    [Fact]
    public async Task GetScheduleById_WhenScheduleExists_ReturnsSchedule()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user.Id}/{schedule.Id}", session.Id);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var returnedSchedule = await response.Content.ReadFromJsonAsync<ScheduleDto>(_jsonOptions);

        Assert.NotNull(returnedSchedule);
        Assert.Equal(schedule.Id, returnedSchedule.Id);
        Assert.Equal(schedule.UserGuid, returnedSchedule.UserId);
    }

    [Fact]
    public async Task GetScheduleById_WhenScheduleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var scheduleId = Guid.NewGuid();

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user.Id}/{scheduleId}", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetScheduleById_WhenScheduleBelongsToDifferentUser_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user1, session1) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext, email: "user1@example.com");
        var user2 = await TestDataSeeder.SeedUserAsync(dbContext, email: "user2@example.com");
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user1.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user2.Id}/{schedule.Id}", session1.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateSchedule_WithValidData_CreatesSchedule()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = new CreateScheduleRequest(
            UserId: user.Id,
            Name: "New Schedule",
            World: WorldType.pl220,
            ScheduleType: ScheduleType.Main,
            []
        );

        var httpRequest = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Post, "/schedules", session.Id);
        httpRequest.Content = JsonContent.Create(request, options: _jsonOptions);
        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdSchedule = await response.Content.ReadFromJsonAsync<ScheduleDto>(_jsonOptions);

        Assert.NotNull(createdSchedule);
        Assert.Equal(request.Name, createdSchedule.Name);
        Assert.Equal(WorldType.pl220, createdSchedule.World);
        Assert.Equal(ScheduleType.Main, createdSchedule.ScheduleType);
        Assert.Equal(user.Id, createdSchedule.UserId);

        var scheduleCount = await TestDataSeeder.GetScheduleCountAsync(dbContext);
        Assert.Equal(1, scheduleCount);
    }

    [Fact]
    public async Task CreateSchedule_ReturnsCreatedLocationHeader()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = new CreateScheduleRequest(
            UserId: user.Id,
            Name: "New Schedule",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Fake,
            []
        );

        var httpRequest = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Post, "/schedules", session.Id);
        httpRequest.Content = JsonContent.Create(request, options: _jsonOptions);
        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var createdSchedule = await response.Content.ReadFromJsonAsync<ScheduleDto>(_jsonOptions);
        Assert.NotNull(createdSchedule);
        Assert.Contains($"/schedules/{createdSchedule.UserId}/{createdSchedule.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreateSchedule_WithInvalidUserId_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = new CreateScheduleRequest(
            UserId: Guid.NewGuid(),
            Name: "New Schedule",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Main,
            []
        );

        var httpRequest = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Post, "/schedules", session.Id);
        httpRequest.Content = JsonContent.Create(request, options: _jsonOptions);
        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSchedule_WithValidData_UpdatesSchedule()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            name: "Original Name",
            world: WorldType.pl218,
            scheduleType: ScheduleType.Fake);

        var updateRequest = new UpdateScheduleRequest(
            Name: "Updated Name",
            World: WorldType.pl221,
            ScheduleType: ScheduleType.Main,
            []
        );

        var httpRequest = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Put, $"/schedules/{schedule.Id}", session.Id);
        httpRequest.Content = JsonContent.Create(updateRequest, options: _jsonOptions);
        var response = await _client.SendAsync(httpRequest);

        response.EnsureSuccessStatusCode();
        var updatedSchedule = await response.Content.ReadFromJsonAsync<ScheduleDto>(_jsonOptions);

        Assert.NotNull(updatedSchedule);
        Assert.Equal(schedule.Id, updatedSchedule.Id);
        Assert.Equal("Updated Name", updatedSchedule.Name);
        Assert.Equal(WorldType.pl221, updatedSchedule.World);
        Assert.Equal(ScheduleType.Main, updatedSchedule.ScheduleType);
    }

    [Fact]
    public async Task UpdateSchedule_WhenScheduleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var scheduleId = Guid.NewGuid();

        var updateRequest = new UpdateScheduleRequest(
            Name: "Updated Name",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Main,
            []
        );

        var httpRequest = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Put, $"/schedules/{scheduleId}", session.Id);
        httpRequest.Content = JsonContent.Create(updateRequest, options: _jsonOptions);
        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSchedule_WhenScheduleExists_DeletesSchedule()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Delete, $"/schedules/{schedule.Id}", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var scheduleCount = await TestDataSeeder.GetScheduleCountAsync(dbContext);
        Assert.Equal(0, scheduleCount);
    }

    [Fact]
    public async Task DeleteSchedule_WhenScheduleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (_, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);
        var scheduleId = Guid.NewGuid();

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Delete, $"/schedules/{scheduleId}", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSchedulesByUser_ReturnsOkStatusCode()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user.Id}", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSchedulesByUser_ReturnsJsonContentType()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(dbContext);

        var request = TestDataSeeder.CreateAuthenticatedRequest(HttpMethod.Get, $"/schedules/{user.Id}", session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}
