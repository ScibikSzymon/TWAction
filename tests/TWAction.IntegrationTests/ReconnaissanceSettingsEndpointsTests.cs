using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Application.Settings.DTOs;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;
using TWAction.IntegrationTests.Helpers;
using TWAction.Persistence;

namespace TWAction.IntegrationTests;

public sealed class ReconnaissanceSettingsEndpointsTests : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReconnaissanceSettingsEndpointsTests(TWActionWebApplicationFactory factory)
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
    public async Task GetReconnaissanceSettings_WhenSettingsExist_ReturnsSettings()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var settings = new ReconnaissanceSettings
        {
            Id = Guid.NewGuid(),
            ScheduleId = schedule.Id,
            MinDepartureTime = DateTimeOffset.UtcNow,
            MinArrivalTime = DateTimeOffset.UtcNow.AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront = 5,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        await dbContext.ReconnaissanceSettings.AddAsync(settings);
        await dbContext.SaveChangesAsync();

        var response = await _client.GetAsync($"/schedules/{schedule.Id}/reconnaissance");

        response.EnsureSuccessStatusCode();
        var returnedSettings = await response.Content.ReadFromJsonAsync<ReconnaissanceSettingsDto>(_jsonOptions);

        Assert.NotNull(returnedSettings);
        Assert.Equal(settings.Id, returnedSettings.Id);
        Assert.Equal(settings.ScheduleId, returnedSettings.ScheduleId);
        Assert.Equal(settings.MinDepartureTime.ToUnixTimeMilliseconds(), returnedSettings.MinDepartureTime.ToUnixTimeMilliseconds());
        Assert.Equal(settings.MinArrivalTime.ToUnixTimeMilliseconds(), returnedSettings.MinArrivalTime.ToUnixTimeMilliseconds());
        Assert.Equal(settings.MaxArrivalTime.ToUnixTimeMilliseconds(), returnedSettings.MaxArrivalTime.ToUnixTimeMilliseconds());
        Assert.Equal(settings.MinDistanceToFront, returnedSettings.MinDistanceToFront);
        Assert.Equal(settings.MinSpyCount, returnedSettings.MinSpyCount);
        Assert.Equal(settings.MaxPopulationInSourceVillage, returnedSettings.MaxPopulationInSourceVillage);
        Assert.Equal(settings.SkipNightSendings, returnedSettings.SkipNightSendings);
    }

    [Fact]
    public async Task GetReconnaissanceSettings_WhenSettingsNotFound_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var response = await _client.GetAsync($"/schedules/{schedule.Id}/reconnaissance");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WithNewSettings_CreatesSettings()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var minDepartureTime = DateTimeOffset.UtcNow;
        var minArrivalTime = minDepartureTime.AddHours(1);
        var maxArrivalTime = minArrivalTime.AddHours(1);

        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = minDepartureTime,
            MinArrivalTime = minArrivalTime,
            MaxArrivalTime = maxArrivalTime,
            MinDistanceToFront = 5,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}/reconnaissance", request, _jsonOptions);

        response.EnsureSuccessStatusCode();
        var returnedSettings = await response.Content.ReadFromJsonAsync<ReconnaissanceSettingsDto>(_jsonOptions);

        Assert.NotNull(returnedSettings);
        Assert.Equal(schedule.Id, returnedSettings.ScheduleId);
        Assert.Equal(minDepartureTime, returnedSettings.MinDepartureTime);
        Assert.Equal(minArrivalTime, returnedSettings.MinArrivalTime);
        Assert.Equal(maxArrivalTime, returnedSettings.MaxArrivalTime);
        Assert.Equal(5, returnedSettings.MinDistanceToFront);
        Assert.Equal(1, returnedSettings.MinSpyCount);
        Assert.Equal(100, returnedSettings.MaxPopulationInSourceVillage);
        Assert.False(returnedSettings.SkipNightSendings);

        var settingsCount = await dbContext.ReconnaissanceSettings.CountAsync();
        Assert.Equal(1, settingsCount);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WithExistingSettings_UpdatesSettings()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var existingSettings = new ReconnaissanceSettings
        {
            Id = Guid.NewGuid(),
            ScheduleId = schedule.Id,
            MinDepartureTime = DateTimeOffset.UtcNow.AddDays(-1),
            MinArrivalTime = DateTimeOffset.UtcNow.AddDays(-1).AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddDays(-1).AddHours(2),
            MinDistanceToFront = 3,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 50,
            SkipNightSendings = false
        };

        await dbContext.ReconnaissanceSettings.AddAsync(existingSettings);
        await dbContext.SaveChangesAsync();

        var minDepartureTime = DateTimeOffset.UtcNow;
        var minArrivalTime = minDepartureTime.AddHours(1);
        var maxArrivalTime = minArrivalTime.AddHours(1);

        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = minDepartureTime,
            MinArrivalTime = minArrivalTime,
            MaxArrivalTime = maxArrivalTime,
            MinDistanceToFront = 10,
            MinSpyCount = 2,
            MaxPopulationInSourceVillage = 200,
            SkipNightSendings = true
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}/reconnaissance", request, _jsonOptions);

        response.EnsureSuccessStatusCode();
        var returnedSettings = await response.Content.ReadFromJsonAsync<ReconnaissanceSettingsDto>(_jsonOptions);

        Assert.NotNull(returnedSettings);
        Assert.Equal(existingSettings.Id, returnedSettings.Id);
        Assert.Equal(schedule.Id, returnedSettings.ScheduleId);
        Assert.Equal(minDepartureTime, returnedSettings.MinDepartureTime);
        Assert.Equal(minArrivalTime, returnedSettings.MinArrivalTime);
        Assert.Equal(maxArrivalTime, returnedSettings.MaxArrivalTime);
        Assert.Equal(10, returnedSettings.MinDistanceToFront);
        Assert.Equal(2, returnedSettings.MinSpyCount);
        Assert.Equal(200, returnedSettings.MaxPopulationInSourceVillage);
        Assert.True(returnedSettings.SkipNightSendings);

        var settingsCount = await dbContext.ReconnaissanceSettings.CountAsync();
        Assert.Equal(1, settingsCount);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WhenScheduleNotFound_ReturnsNotFound()
    {
        var scheduleId = Guid.NewGuid();
        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = DateTimeOffset.UtcNow,
            MinArrivalTime = DateTimeOffset.UtcNow.AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront = 5,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{scheduleId}/reconnaissance", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WhenScheduleTypeIsNotReconnaissance_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Main);

        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = DateTimeOffset.UtcNow,
            MinArrivalTime = DateTimeOffset.UtcNow.AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront = 5,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}/reconnaissance", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WithInvalidTimeConstraints_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var minDepartureTime = DateTimeOffset.UtcNow;

        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = minDepartureTime,
            MinArrivalTime = minDepartureTime,
            MaxArrivalTime = minDepartureTime,
            MinDistanceToFront = 5,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}/reconnaissance", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WithNegativeMinDistanceToFront_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = DateTimeOffset.UtcNow,
            MinArrivalTime = DateTimeOffset.UtcNow.AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront = -1,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}/reconnaissance", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WithInvalidMinSpyCount_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = DateTimeOffset.UtcNow,
            MinArrivalTime = DateTimeOffset.UtcNow.AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront = 5,
            MinSpyCount = 0,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}/reconnaissance", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SaveReconnaissanceSettings_WithNegativeMaxPopulation_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var user = await TestDataSeeder.SeedUserAsync(dbContext);
        var schedule = await TestDataSeeder.SeedScheduleAsync(
            dbContext,
            userId: user.Id,
            scheduleType: ScheduleType.Reconnaissance);

        var request = new SaveReconnaissanceSettingsRequest
        {
            MinDepartureTime = DateTimeOffset.UtcNow,
            MinArrivalTime = DateTimeOffset.UtcNow.AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront = 5,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = -1,
            SkipNightSendings = false
        };

        var response = await _client.PutAsJsonAsync($"/schedules/{schedule.Id}/reconnaissance", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
