using TWAction.Domain.Users;

namespace TWAction.Application.Users.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> FindByEmailAsync(string email, string provider, CancellationToken cancellationToken = default);

    Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserEntity>> ListAllAsync(CancellationToken cancellationToken = default);
    
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserEntity> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);

    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
