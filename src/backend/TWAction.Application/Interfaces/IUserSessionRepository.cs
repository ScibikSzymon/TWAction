using TWAction.Domain.Entities;

namespace TWAction.Application.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSessionEntity> CreateSessionAsync(UserSessionEntity session, CancellationToken cancellationToken = default);

    Task<UserSessionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
