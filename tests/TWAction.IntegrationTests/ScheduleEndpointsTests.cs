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
        var userId = Guid.NewGuid();

        var response = await _client.GetAsync($"/schedules/{userId}");

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
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        await TestDataSeeder.SeedMultipleSchedulesAsync(dbContext, user.Id, 3);

        var response = await _client.GetAsync($"/schedules/{user.Id}");

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
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var seededSchedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            name: "Test Schedule",
            world: WorldType.pl219,
            scheduleType: ScheduleType.Reconnaissance);

        var response = await _client.GetAsync($"/schedules/{user.Id}");

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
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user.Id);

        var response = await _client.GetAsync($"/schedules/{user.Id}/{schedule.Id}");

        response.EnsureSuccessStatusCode();
        var returnedSchedule = await response.Content.ReadFromJsonAsync<ScheduleDto>(_jsonOptions);

        Assert.NotNull(returnedSchedule);
        Assert.Equal(schedule.Id, returnedSchedule.Id);
        Assert.Equal(schedule.UserGuid, returnedSchedule.UserId);
    }

    [Fact]
    public async Task GetScheduleById_WhenScheduleDoesNotExist_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();

        var response = await _client.GetAsync($"/schedules/{userId}/{scheduleId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetScheduleById_WhenScheduleBelongsToDifferentUser_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user1 = await TestDataSeeder.SeedUserAsync(dbContext, email: "user1@example.com");
        var user2 = await TestDataSeeder.SeedUserAsync(dbContext, email: "user2@example.com");
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user1.Id);

        var response = await _client.GetAsync($"/schedules/{user2.Id}/{schedule.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateSchedule_WithValidData_CreatesSchedule()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);

        var request = new CreateScheduleRequest(
            UserId: user.Id,
            Name: "New Schedule",
            World: WorldType.pl220.ToString(),
            ScheduleType: ScheduleType.Main.ToString(),
            []
        );

        var response = await _client.PostAsJsonAsync("/schedules", request, _jsonOptions);

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
        var user = await TestDataSeeder.SeedUserAsync(dbContext);

        var request = new CreateScheduleRequest(
            UserId: user.Id,
            Name: "New Schedule",
            World: WorldType.pl218.ToString(),
            ScheduleType: ScheduleType.Fake.ToString(),
            []
        );

        var response = await _client.PostAsJsonAsync("/schedules", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var createdSchedule = await response.Content.ReadFromJsonAsync<ScheduleDto>(_jsonOptions);
        Assert.NotNull(createdSchedule);
        Assert.Contains($"/schedules/{createdSchedule.UserId}/{createdSchedule.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreateSchedule_WithInvalidUserId_ReturnsBadRequest()
    {
        var request = new CreateScheduleRequest(
            UserId: Guid.NewGuid(),
            Name: "New Schedule",
            World: WorldType.pl218.ToString(),
            ScheduleType: ScheduleType.Main.ToString(),
            []
        );

        var response = await _client.PostAsJsonAsync("/schedules", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSchedule_WithValidData_UpdatesSchedule()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            name: "Original Name",
            world: WorldType.pl218,
            scheduleType: ScheduleType.Fake);

        var updateRequest = new UpdateScheduleRequest(
            Name: "Updated Name",
            World: WorldType.pl221.ToString(),
            ScheduleType: ScheduleType.Main.ToString(),
            []
        );

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}", updateRequest, _jsonOptions);

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
        var scheduleId = Guid.NewGuid();
        var updateRequest = new UpdateScheduleRequest(
            Name: "Updated Name",
            World: WorldType.pl218.ToString(),
            ScheduleType: ScheduleType.Main.ToString(),
            []
        );

        var response = await _client.PutAsJsonAsync($"/schedules/{scheduleId}", updateRequest, _jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSchedule_WhenScheduleExists_DeletesSchedule()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(dbContext, userId: user.Id);

        var response = await _client.DeleteAsync($"/schedules/{schedule.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var scheduleCount = await TestDataSeeder.GetScheduleCountAsync(dbContext);
        Assert.Equal(0, scheduleCount);
    }

    [Fact]
    public async Task DeleteSchedule_WhenScheduleDoesNotExist_ReturnsNotFound()
    {
        var scheduleId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/schedules/{scheduleId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSchedulesByUser_ReturnsOkStatusCode()
    {
        var userId = Guid.NewGuid();

        var response = await _client.GetAsync($"/schedules/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSchedulesByUser_ReturnsJsonContentType()
    {
        var userId = Guid.NewGuid();

        var response = await _client.GetAsync($"/schedules/{userId}");

        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}
