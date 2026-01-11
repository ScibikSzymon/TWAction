using Microsoft.EntityFrameworkCore;
using TWAction.Application.Interfaces;
using TWAction.Domain.Entities;

namespace TWAction.Persistence.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly TWActionDbContext _db;

    public UserSessionRepository(TWActionDbContext db) => _db = db;

    public async Task<UserSessionEntity> CreateSessionAsync(UserSessionEntity session, CancellationToken cancellationToken = default)
    {
        await _db.UserSessions.AddAsync(session, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<UserSessionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.UserSessions.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expired = await _db.UserSessions.Where(s => s.ExpiresAt < DateTimeOffset.UtcNow).ToListAsync(cancellationToken);
        if (expired.Any())
        {
            _db.UserSessions.RemoveRange(expired);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _db.UserSessions.FindAsync(new object[] { id }, cancellationToken).AsTask();
        if (session is null) return;
        _db.UserSessions.Remove(session);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
