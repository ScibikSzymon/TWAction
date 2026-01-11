using Microsoft.EntityFrameworkCore;
using TWAction.Application.Interfaces;
using TWAction.Domain.Entities;

namespace TWAction.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TWActionDbContext _db;

    public UserRepository(TWActionDbContext db) => _db = db;

    public async Task<UserEntity?> FindByEmailAsync(string email, string provider, CancellationToken cancellationToken = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Provider == provider, cancellationToken);
    }

    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<IReadOnlyList<UserEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }
}
