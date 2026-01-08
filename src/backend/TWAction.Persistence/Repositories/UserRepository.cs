using Microsoft.EntityFrameworkCore;
using TWAction.Application.Interfaces;
using TWAction.Domain.Entities;

namespace TWAction.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TWActionDbContext _db;

    public UserRepository(TWActionDbContext db) => _db = db;

    public async Task<User?> FindByEmailAsync(string email, string provider, CancellationToken cancellationToken = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Provider == provider, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<IReadOnlyList<User>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking().ToListAsync(cancellationToken);
    }
}
