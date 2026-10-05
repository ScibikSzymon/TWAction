using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Api.Endpoints;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Templates.DTOs;
using TWAction.Domain.Schedules;
using TWAction.Domain.Templates;
using TWAction.Domain.Users;
using TWAction.IntegrationTests.Helpers;
using TWAction.Persistence;

namespace TWAction.IntegrationTests;

public sealed class SubscriptionLimitsEndpointsTests
    : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SubscriptionLimitsEndpointsTests(TWActionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _factory.ResetDatabaseAsync();

    [Fact]
    public async Task GetDefaultLimits_WithoutAuthentication_ReturnsDatabaseConfiguration()
    {
        var response = await _client.GetAsync("/about/limits");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var plans = await response.Content.ReadFromJsonAsync<List<PlanLimitsResponse>>(_jsonOptions);

        Assert.NotNull(plans);
        var free = Assert.Single(plans, plan => plan.SubscriptionTier == "Free");
        Assert.Equal(3, free.ScheduleLimit);
        Assert.Equal(1, free.TemplateLimit);
        Assert.Equal(2, free.TroopsUploadLimit);
        Assert.Equal(12, free.TroopsUploadWindowHours);

        var premium = Assert.Single(plans, plan => plan.SubscriptionTier == "Premium");
        Assert.Null(premium.ScheduleLimit);
        Assert.Null(premium.TemplateLimit);
        Assert.Null(premium.TroopsUploadLimit);
    }

    [Fact]
    public async Task GetMyLimits_ReturnsCurrentUsageAndEffectiveLimits()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(db);
        await TestDataSeeder.SeedMultipleSchedulesAsync(db, user.Id, 2);
        await SeedTemplateAsync(db, user.Id);

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/users/me/limits",
            session.Id);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var limits = await response.Content.ReadFromJsonAsync<UserLimitsResponse>(_jsonOptions);

        Assert.NotNull(limits);
        Assert.Equal("Free", limits.SubscriptionTier);
        Assert.Equal(3, limits.ScheduleLimit);
        Assert.Equal(2, limits.ScheduleCount);
        Assert.Equal(1, limits.TemplateLimit);
        Assert.Equal(1, limits.TemplateCount);
        Assert.Equal(2, limits.TroopsUploadLimit);
        Assert.Equal(12, limits.TroopsUploadWindowHours);
    }

    [Fact]
    public async Task CreateSchedule_WhenFreeLimitIsReached_ReturnsConflict()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(db);
        await TestDataSeeder.SeedMultipleSchedulesAsync(db, user.Id, 3);

        var body = new CreateScheduleRequest(
            "Over limit",
            WorldType.pl218,
            ScheduleType.Main,
            []);
        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Post,
            "/schedules",
            session.Id);
        request.Content = JsonContent.Create(body, options: _jsonOptions);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(3, await db.Schedules.CountAsync(schedule => schedule.UserGuid == user.Id));
        await AssertLimitReachedErrorAsync(response);
    }

    [Fact]
    public async Task CreateTemplate_WhenFreeLimitIsReached_ReturnsConflict()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(db);
        await SeedTemplateAsync(db, user.Id);

        var body = new CreateTargetTemplateRequest(
            "Over limit",
            [new TemplateWaveDto(
                new TimeOnly(10, 0),
                new TimeOnly(11, 0),
                1,
                CommandTypeConstants.Off)]);
        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Post,
            "/target-templates",
            session.Id);
        request.Content = JsonContent.Create(body, options: _jsonOptions);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await db.TargetTemplates.CountAsync(template => template.UserId == user.Id));
        await AssertLimitReachedErrorAsync(response);
    }

    [Fact]
    public async Task UploadTroopsState_WhenFreeLimitIsReached_ReturnsConflict()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TWActionDbContext>();
        var (user, session) = await TestDataSeeder.SeedUserWithSessionAsync(db);
        var schedule = await TestDataSeeder.SeedScheduleAsync(db, user.Id);
        var now = DateTimeOffset.UtcNow;

        db.TroopsUploads.AddRange(
            CreateTroopsUpload(user.Id, schedule.Id, now.AddHours(-2)),
            CreateTroopsUpload(user.Id, schedule.Id, now.AddHours(-1)));
        await db.SaveChangesAsync();

        var request = TestDataSeeder.CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/schedules/{schedule.Id}/troops",
            session.Id);
        request.Content = JsonContent.Create(
            new UploadTroopsStateRequest { RawData = "not parsed when limit is reached" },
            options: _jsonOptions);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(2, await db.TroopsUploads.CountAsync(upload => upload.ScheduleId == schedule.Id));
        await AssertLimitReachedErrorAsync(response);
    }

    private static async Task SeedTemplateAsync(TWActionDbContext db, Guid userId)
    {
        db.TargetTemplates.Add(new TargetTemplate
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Existing template",
            IsDefault = false,
            Waves = []
        });
        await db.SaveChangesAsync();
    }

    private static TroopsUploadEntity CreateTroopsUpload(
        Guid userId,
        Guid scheduleId,
        DateTimeOffset uploadedAt) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ScheduleId = scheduleId,
            UploadedAt = uploadedAt
        };

    private static async Task AssertLimitReachedErrorAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var error = json.RootElement.GetProperty("error").GetString();
        Assert.Contains("limit reached", error, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record PlanLimitsResponse(
        string SubscriptionTier,
        int? ScheduleLimit,
        int? TemplateLimit,
        int? TroopsUploadLimit,
        int TroopsUploadWindowHours);

    private sealed record UserLimitsResponse(
        string SubscriptionTier,
        int? ScheduleLimit,
        int ScheduleCount,
        int? TemplateLimit,
        int TemplateCount,
        int? TroopsUploadLimit,
        int TroopsUploadWindowHours);
}
