using Microsoft.EntityFrameworkCore;
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
        CancellationToken cancellationToken = default)
    {
        var user = new UserEntityBuilder()
            .WithEmail(email)
            .WithDisplayName(displayName)
            .WithProvider(provider)
            .Build();

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
}
