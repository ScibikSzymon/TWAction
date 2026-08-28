using Microsoft.EntityFrameworkCore;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Users;

namespace TWAction.Persistence.Repositories;

public class UserSessionRepository(TWActionDbContext db) : IUserSessionRepository
{
    public async Task<UserSessionEntity> CreateSessionAsync(UserSessionEntity session, CancellationToken cancellationToken = default)
    {
        await db.UserSessions.AddAsync(session, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<UserSessionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.UserSessions.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<UserSessionEntity>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expired = await db.UserSessions.Where(s => s.ExpiresAt < DateTimeOffset.UtcNow).ToListAsync(cancellationToken);
        if (expired.Any())
        {
            db.UserSessions.RemoveRange(expired);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await db.UserSessions.FindAsync(new object[] { id }, cancellationToken).AsTask();
        if (session is null)
        {
            return;
        }

        db.UserSessions.Remove(session);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await db.UserSessions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return;
        }

        db.UserSessions.RemoveRange(sessions);
        await db.SaveChangesAsync(cancellationToken);
    }
}
