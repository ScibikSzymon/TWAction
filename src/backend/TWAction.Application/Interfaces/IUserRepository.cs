using TWAction.Domain.Entities;

namespace TWAction.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, string provider, CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListAllAsync(CancellationToken cancellationToken = default);
}
