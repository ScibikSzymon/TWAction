using Microsoft.EntityFrameworkCore;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Users;

namespace TWAction.Persistence.Repositories;

public class UserRepository(TWActionDbContext db) : IUserRepository
{
    public async Task<UserEntity?> FindByEmailAsync(string email, string provider, CancellationToken cancellationToken = default)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Provider == provider, cancellationToken);
    }

    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<IReadOnlyList<UserEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Users.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Users.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }
}
