using TWAction.Domain.Entities;

namespace TWAction.Application.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSession> CreateSessionAsync(UserSession session, CancellationToken cancellationToken = default);

    Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
