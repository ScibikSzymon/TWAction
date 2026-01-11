using TWAction.Domain.Entities;

namespace TWAction.Application.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> FindByEmailAsync(string email, string provider, CancellationToken cancellationToken = default);

    Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserEntity>> ListAllAsync(CancellationToken cancellationToken = default);
}
