using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Schedules;
using TWAction.Domain.Users;
using TWAction.Persistence;

namespace TWAction.IntegrationTests.Helpers;

public static class TestDataSeeder
{
    public static async Task<UserEntity> SeedUserAsync(
        TWActionDbContext dbContext,
        string email = "test@example.com",
        string? displayName = "Test User",
        string provider = "google",
        UserRole role = UserRole.User,
        CancellationToken cancellationToken = default)
    {
        var user = new UserEntityBuilder()
            .WithEmail(email)
            .WithDisplayName(displayName)
            .WithProvider(provider)
            .Build();

        user.Role = role;

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    public static async Task<List<UserEntity>> SeedMultipleUsersAsync(
        TWActionDbContext dbContext,
        int count,
        CancellationToken cancellationToken = default)
    {
        var users = new List<UserEntity>();

        for (int i = 0; i < count; i++)
        {
            var user = new UserEntityBuilder()
                .WithEmail($"user{i}@example.com")
                .WithDisplayName($"Test User {i}")
                .Build();

            users.Add(user);
        }

        await dbContext.Users.AddRangeAsync(users, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return users;
    }

    public static async Task<int> GetUserCountAsync(
        TWActionDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.CountAsync(cancellationToken);
    }

    public static async Task<ScheduleEntity> SeedScheduleAsync(
        TWActionDbContext dbContext,
        Guid? userId = null,
        string name = "Test Schedule",
        WorldType world = WorldType.pl218,
        ScheduleType scheduleType = ScheduleType.Main,
        CancellationToken cancellationToken = default)
    {
        var schedule = new ScheduleEntityBuilder()
            .WithUserGuid(userId ?? Guid.NewGuid())
            .WithName(name)
            .WithWorld(world)
            .WithScheduleType(scheduleType)
            .Build();

        await dbContext.Schedules.AddAsync(schedule, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return schedule;
    }

    public static async Task<List<ScheduleEntity>> SeedMultipleSchedulesAsync(
        TWActionDbContext dbContext,
        Guid userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        var schedules = new List<ScheduleEntity>();

        for (int i = 0; i < count; i++)
        {
            var schedule = new ScheduleEntityBuilder()
                .WithUserGuid(userId)
                .WithName($"Test Schedule {i}")
                .WithWorld((WorldType)(i % Enum.GetValues<WorldType>().Length))
                .WithScheduleType((ScheduleType)(i % Enum.GetValues<ScheduleType>().Length))
                .Build();

            schedules.Add(schedule);
        }

        await dbContext.Schedules.AddRangeAsync(schedules, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return schedules;
    }

    public static async Task<int> GetScheduleCountAsync(
        TWActionDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Schedules.CountAsync(cancellationToken);
    }

    public static async Task<UserSessionEntity> SeedSessionAsync(
        TWActionDbContext dbContext,
        Guid userId,
        Guid? sessionId = null,
        DateTimeOffset? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        var session = new SessionEntityBuilder()
            .WithId(sessionId ?? Guid.NewGuid())
            .WithUserId(userId)
            .WithExpiresAt(expiresAt ?? DateTimeOffset.UtcNow.AddHours(8))
            .Build();

        await dbContext.UserSessions.AddAsync(session, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return session;
    }

    public static async Task<(UserEntity User, UserSessionEntity Session)> SeedUserWithSessionAsync(
        TWActionDbContext dbContext,
        string email = "test@example.com",
        string? displayName = "Test User",
        string provider = "google",
        UserRole role = UserRole.User,
        CancellationToken cancellationToken = default)
    {
        var user = await SeedUserAsync(dbContext, email, displayName, provider, role, cancellationToken);
        var session = await SeedSessionAsync(dbContext, user.Id, cancellationToken: cancellationToken);

        return (user, session);
    }

    public static HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string uri, Guid sessionId, string cookieName = "TWAction.Session")
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("Cookie", $"{cookieName}={sessionId}");
        return request;
    }

    public static async Task<TroopsStateEntity> SeedTroopsStateAsync(
        TWActionDbContext dbContext,
        Guid scheduleId,
        string? compressedData = null,
        CancellationToken cancellationToken = default)
    {
        compressedData ??= CreateValidCompressedTroopsData();

        var troopsState = new TroopsStateEntity
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            CompressedData = compressedData,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await dbContext.TroopsStates.AddAsync(troopsState, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return troopsState;
    }

    /// <summary>
    /// Creates valid compressed troops data with 2 players and 3 villages.
    /// </summary>
    public static string CreateValidCompressedTroopsData()
    {
        const string rawData =
            "PlayerName,Village,Spear,Sword,Archer,Marcher,Catapult,Axe,Polearm,Ram,Trebuchet\n" +
            "Player1,500|500,100,200,50,30,10,150,80,20,5\n" +
            "Player1,501|501,80,150,40,20,8,120,60,15,3\n" +
            "Player2,502|502,200,300,100,50,20,250,100,30,10";

        var compressionService = new TWAction.Application.Schedules.Services.TroopsStateCompressionService();
        return compressionService.Compress(rawData);
    }
}
