using TWAction.Domain.Users;

namespace TWAction.Application.Users.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSessionEntity> CreateSessionAsync(UserSessionEntity session, CancellationToken cancellationToken = default);

    Task<UserSessionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);

    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
